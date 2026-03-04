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

class Client:
    def __init__(self, channel: Channel) -> None:
        self.client = gmodels.TDFSimulationStub(channel)

    def DoStep(self, id: int, actions: Iterable[TDFDroneAction]) -> TDFDoStepResponse:
        return self.client.DoStep(TDFDoStepRequest(id=id, drone_actions=actions))

    def New(self) -> TDFNewResponse:
        return self.client.New(TDFNewRequest())

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
