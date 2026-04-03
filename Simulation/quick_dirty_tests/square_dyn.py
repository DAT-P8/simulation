import random
import grpc
import logging
from ngw.v1.ngw2d_pb2 import (
    Action,
    DoStepRequest,
    DoStepResponse,
    DroneAction,
    ResetRequest,
    ResetResponse,
    StateResponse,
    MapSpec,
    NewRequest,
    NewResponse,
    SquareMap,
)
from ngw.v1.ngw2d_pb2_grpc import (
    SimulationServiceStub
)
from grpc import Channel

class SimulationClient:
    def __init__(self, chan: Channel) -> None:
        self.stub = SimulationServiceStub(chan)

    def new_square_map(
        self,
        evader_count: int = 10,
        pursuer_count: int = 10,
        drone_velocity: int = 2,
        width: int = 10,
        height: int = 10,
        target_x: int = 5,
        target_y: int = 5
    ) -> NewResponse:
        square_map = MapSpec(
            square_map=SquareMap(width=width, height=height, target_x=target_x, target_y=target_y)
        )
        return self.stub.New(NewRequest(
            map=square_map,
            evader_count=evader_count,
            pursuer_count=pursuer_count,
            drone_velocity=drone_velocity,
        ))

    def do_step(self, sim_id: int, actions: list[DroneAction]) -> DoStepResponse:
        return self.stub.DoStep(DoStepRequest(drone_actions=actions, sim_id=sim_id))

    def reset(self, sim_id: int) -> ResetResponse:
        return self.stub.Reset(ResetRequest(sim_id=sim_id))

class EndlessRandomSquareSimulation:
    def __init__(
        self,
        chan: Channel,
        evader_count: int = 10,
        pursuer_count: int = 10,
        drone_velocity: int = 2,
        width: int = 10,
        height: int = 10,
        target_x: int = 5,
        target_y: int = 5,
    ) -> None:
        self.client = SimulationClient(chan)
        self.evader_count = evader_count
        self.pursuer_count = pursuer_count
        self.drone_velocity = drone_velocity
        self.width = width
        self.height = height
        self.target_x = target_x
        self.target_y = target_y

        logging.info(f"Creating new simulation")
        response = self.client.new_square_map(
            evader_count=self.evader_count,
            pursuer_count=self.pursuer_count,
            drone_velocity=self.drone_velocity,
            width=self.width,
            height=self.height,
            target_x=self.target_x,
            target_y=self.target_y,
        )
        if has_error(response.state_response):
            raise Exception(f"Failed initializing new simulation: {response.state_response.error_message}")

        self.state = response.state_response.state
        logging.info(f"New simulation {self.state.sim_id} successfully created")


    def is_terminated(self):
        return self.state.terminated
    

    def progress(self) -> bool:
        if self.is_terminated():
            logging.info(f"Simulation {self.state.sim_id} terminated, resetting")
            r = self.client.reset(self.state.sim_id)
            if has_error(r.state_response):
                raise Exception(f"Exception resetting simulation {self.state.sim_id}: {r.state_response.error_message}")
            self.state = r.state_response.state

        logging.info(f"Doing step of simulation {self.state.sim_id}")
        r = self.client.do_step(self.state.sim_id, [DroneAction(id=x.id, action=random_action()) for x in self.state.drone_states])
        if has_error(r.state_response):
            raise Exception(f"Exception doing step of simulation {self.state.sim_id}: {r.state_response.error_message}")

        return False


def random_action():
    randint = random.randrange(1, 10)
    return Action(randint)


def has_error(response: StateResponse) -> bool:
    return response.WhichOneof("state_or_error") == "error_message"


def main() -> None:
    logging.basicConfig(level=logging.INFO)
    channel = grpc.insecure_channel("localhost:50051");
    simulation = EndlessRandomSquareSimulation(chan=channel)

    while not simulation.progress():
        pass


if __name__ == "__main__":
    main()
