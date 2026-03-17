from typing import Any, Iterable
import random
import numpy as np
import TDF_pb2_grpc as gmodels
from TDF_pb2 import (
    TDFCloseRequest,
    TDFCloseResponse,
    TDFDoStepRequest,
    TDFDoStepResponse,
    TDFDroneAction,
    TDFNewRequest,
    TDFNewResponse,
    TDFResetRequest,
    TDFResetResponse
)
from grpc import Channel
import grpc

class Client:
    def __init__(self, channel: Channel) -> None:
        self.client = gmodels.TDFSimulationStub(channel)

    def DoStep(self, id: int, actions: Iterable[TDFDroneAction]) -> TDFDoStepResponse:
        return self.client.DoStep(TDFDoStepRequest(id=id, drone_actions=actions))

    def New(self, evader_count: int, pursuer_count: int, evader_dome_radius: float, pursuer_dome_radius: float, arena_dome_radius: float, drone_max_speed: float, seed: int) -> TDFNewResponse:
        return self.client.New(TDFNewRequest(evader_count=evader_count, pursuer_count=pursuer_count, evader_dome_radius=evader_dome_radius, pursuer_dome_radius=pursuer_dome_radius, arena_dome_radius=arena_dome_radius, drone_max_speed=drone_max_speed, seed=seed))

    def Reset(self, id: int) -> TDFResetResponse:
        return self.client.Reset(TDFResetRequest(id=id))

    def Close(self, id: int) -> TDFCloseResponse:
        return self.client.Close(TDFCloseRequest(id=id))

class RandSimulation:
    def __init__(self, client: Client) -> None:
        self.force_scale = .01
        self.client = client

        response = self.client.New(
            evader_count=10,
            pursuer_count=10,
            evader_dome_radius=20,
            pursuer_dome_radius=50,
            arena_dome_radius=100,
            drone_max_speed=1,
            seed=random.randint(0, 1000)
        )

        if response.WhichOneof("error_case") == "error_msg":
            raise Exception(f"Received error response: {response.error_msg}")
        if response.state.terminated:
            raise Exception("Simulation terminated at initialization")

        self.id = response.state.sim_id
        self.drones = [d for d in response.state.drone_states]
        self.terminated = response.state.terminated


    def __set_state__(self, some: Any):
        self.id = some.state.sim_id
        self.drones = [d for d in some.state.drone_states]
        self.terminated = some.state.terminated


    def get_random_action(self, id: int) -> TDFDroneAction:
        return TDFDroneAction(id=id, x_f=(random.random() - .5) * self.force_scale, y_f=(random.random() - .5) * self.force_scale, z_f=(random.random() - .5) * self.force_scale)


    def Progress(self):
        if self.terminated:
            response = self.client.Reset(self.id)
            self.__set_state__(response)

        self.__set_state__(self.client.DoStep(self.id, [self.get_random_action(d.id) for d in self.drones]))

        return self.terminated


class CollidingSimulation:
    def __init__(self, client: Client) -> None:
        self.force_scale = 1
        self.client = client

        response = self.client.New(
            evader_count=20,
            pursuer_count=20,
            evader_dome_radius=50,
            pursuer_dome_radius=50,
            arena_dome_radius=100,
            drone_max_speed=5,
            seed=random.randint(0, 1000)
        )

        if response.WhichOneof("error_case") == "error_msg":
            raise Exception(f"Received error response: {response.error_msg}")
        if response.state.terminated:
            raise Exception("Simulation terminated at initialization")

        self.id = response.state.sim_id
        self.state = response.state

    def Progress(self):
        if self.state.terminated:
            self.state = self.client.Reset(self.id).state

        actions = []
        for ds in self.state.drone_states:
            id = ds.id
            vf = np.array([-ds.x, -ds.y, -ds.z])
            vf = vf / np.linalg.norm(vf)
            xf = vf[0] * self.force_scale
            yf = vf[1] * self.force_scale
            zf = vf[2] * self.force_scale
            action = TDFDroneAction(id=id, x_f=xf, y_f=yf, z_f=zf)
            actions.append(action)


        response = self.client.DoStep(id=self.id, actions=actions)

        if response.WhichOneof("error_case") == "error_msg":
            raise Exception(f"Received error response: {response.error_msg}")

        self.state = response.state


def main():
    channel = grpc.insecure_channel("localhost:50051");
    client = Client(channel)
    simulations = [CollidingSimulation(client) for _ in range(10)]

    while True:
        for sim in simulations:
            if sim.Progress():
                pass


if __name__ == "__main__":
    main()
