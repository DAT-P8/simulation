import random
import time
import grpc
import logging
from ngw.v1.ngw2d_pb2 import (
    ACTION_DOWN,
    ACTION_LEFT,
    ACTION_LEFT_DOWN,
    ACTION_LEFT_UP,
    ACTION_NOTHING,
    ACTION_RIGHT,
    ACTION_RIGHT_DOWN,
    ACTION_RIGHT_UP,
    ACTION_UP,
    DoStepRequest,
    DoStepResponse,
    DroneAction,
    Event,
    ObjectSpec,
    ResetRequest,
    ResetResponse,
    SquareObject,
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
        width: int = 3,
        height: int = 3,
        target_x: int = 1,
        target_y: int = 1,
        objects: list[tuple[int, int]] = []
    ) -> NewResponse:
        dto_objects = [ObjectSpec(square_object=SquareObject(x=x, y=y)) for x, y in objects]
        square_map = MapSpec(
            square_map=SquareMap(
                width=width,
                height=height,
                target_x=target_x,
                target_y=target_y,
                objects=dto_objects
            )
        )

        return self.stub.New(NewRequest(
            map=square_map,
            evader_count=evader_count,
            pursuer_count=pursuer_count
        ))

    def do_step(self, sim_id: int, actions: list[DroneAction]) -> DoStepResponse:
        return self.stub.DoStep(DoStepRequest(drone_actions=actions, sim_id=sim_id))

    def reset(self, sim_id: int) -> ResetResponse:
        return self.stub.Reset(ResetRequest(sim_id=sim_id))

class EndlessRandomSquareSimulation:
    def __init__(
        self,
        chan: Channel,
        evader_count: int = 2,
        pursuer_count: int = 2,
        drone_velocity: int = 2,
        width: int = 11,
        height: int = 11,
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
        self.collision_set: set[int] = set()

        logging.info(f"Creating new simulation")
        response = self.client.new_square_map(
            evader_count=self.evader_count,
            pursuer_count=self.pursuer_count,
            drone_velocity=self.drone_velocity,
            width=self.width,
            height=self.height,
            target_x=self.target_x,
            target_y=self.target_y,
            objects=[(0,0), (1,1), (2,2), (3,3), (4,4), (5,5), (6,6), (7,7), (8,8), (9,9)]
        )
        if has_error(response.state_response):
            raise Exception(f"Failed initializing new simulation: {response.state_response.error_message}")

        self.state = response.state_response.state
        logging.info(f"New simulation {self.state.sim_id} successfully created")


    def is_terminated(self):
        return self.state.terminated
    

    def progress(self) -> bool:
        if self.is_terminated():
            self.collision_set = set()
            r = self.client.reset(self.state.sim_id)
            if has_error(r.state_response):
                raise Exception(f"Exception resetting simulation {self.state.sim_id}: {r.state_response.error_message}")
            self.state = r.state_response.state

        r = self.client.do_step(self.state.sim_id, [DroneAction(id=x.id, action=random_action(), velocity=random.randrange(1, 3)) for x in self.state.drone_states])
        if has_error(r.state_response):
            raise Exception(f"Exception doing step of simulation {self.state.sim_id}: {r.state_response.error_message}")

        self.state = r.state_response.state

          # oneof event_oneof {
          #   CollisionEvent collision_event = 1;
          #   TargetReachedEvent target_reached_event = 2;
          #   OutOfBoundsEvent out_of_bounds_event = 3;
          #   PursuerEnteredTargetEvent pursuer_entered_target_event = 4;
          #   DroneObjectCollisionEvent drone_object_collision_event = 5;
          # }

        for e in self.state.events:
            whichoneof = e.WhichOneof("event_oneof")
            print(whichoneof)

        return False


def random_action():
    randint = random.randrange(1, 10)

    if randint == 1:
        return ACTION_DOWN
    if randint == 2:
        return ACTION_LEFT_DOWN
    if randint == 3:
        return ACTION_LEFT
    if randint == 4:
        return ACTION_LEFT_UP
    if randint == 5:
        return ACTION_UP
    if randint == 6:
        return ACTION_RIGHT_UP
    if randint == 7:
        return ACTION_RIGHT
    if randint == 8:
        return ACTION_RIGHT_DOWN

    return ACTION_NOTHING

def event_is_collision(e: Event):
    return e.WhichOneof("event_oneof") == "collision_event"

def event_is_target_reached(e: Event):
    return e.WhichOneof("event_oneof") == "target_reached_event"

def event_is_out_of_bounds(e: Event):
    return e.WhichOneof("event_oneof") == "out_of_bounds_event"

def event_is_pursuer_entered_target(e: Event):
    return e.WhichOneof("event_oneof") == "pursuer_entered_target_event"

def has_error(response: StateResponse) -> bool:
    return response.WhichOneof("state_or_error") == "error_message"


def main() -> None:
    logging.basicConfig(level=logging.INFO)
    channel = grpc.insecure_channel("localhost:50051");

    time.sleep(1)

    simulation = EndlessRandomSquareSimulation(chan=channel)

    while not simulation.progress():
        # time.sleep(.5)
        pass


if __name__ == "__main__":
    main()
