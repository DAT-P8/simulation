from typing import Iterable
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

    def New(self) -> TDFNewResponse:
        return self.client.New(TDFNewRequest(evader_count=5, pursuer_count=5, evader_dome_radius=10, pursuer_dome_radius=5, arena_dome_radius=30))

    def Reset(self, id: int) -> TDFResetResponse:
        return self.client.Reset(TDFResetRequest(id=id))

    def Close(self, id: int) -> TDFCloseResponse:
        return self.client.Close(TDFCloseRequest(id=id))

class Simulation:
    def __init__(self, client: Client) -> None:
        self.client = client
        response = self.client.New()
        if response.WhichOneof("error_case") == "error_msg":
            raise Exception(f"Received error response: {response.error_msg}")
        if response.state.terminated:
            raise Exception("Simulation terminated at initialization")

        self.id = response.state.sim_id
        self.drones = [d for d in response.state.drone_states]

    def Progress(self):
        pass


def main():
    channel = grpc.insecure_channel("localhost:50051");
    client = Client(channel)
    simulations = [Simulation(client) for _ in range(10)]

    while True:
        for sim in simulations:
            if sim.Progress():
                pass


if __name__ == "__main__":
    main()
