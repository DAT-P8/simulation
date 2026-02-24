import grid_world_pb2 as pmodels
import time
import random
import grid_world_pb2_grpc as gmodels
import grpc

def run():
    channel = grpc.insecure_channel("localhost:50051");
    client = Client(channel)
    simulations = [Simulation(client) for _ in range(1)]

    while True:
        for sim in simulations:
            time.sleep(.2)
            if sim.Progress():
                pass


class Client:
    def __init__(self, channel: gmodels.grpc.Channel) -> None:
        self.stub = gmodels.GWSimulationStub(channel)

    def Reset(self, request: pmodels.GWResetRequest) -> pmodels.GWResetResponse:
        return self.stub.Reset(request)
    
    def DoStep(self, request: pmodels.GWActionRequest) -> pmodels.GWActionResponse:
        return self.stub.DoStep(request)
    
    def New(self) -> pmodels.GWNewResponse:
        return self.stub.New(pmodels.GWNewRequest())
    
    def Close(self, request: pmodels.GWCloseRequest) -> pmodels.GWCloseResponse:
        return self.stub.Close(request)


class Simulation:
    def __init__(self, client: Client) -> None:
        self.client = client
        response = self.client.New();
        self.state = response.state
        self.id = response.id

    def Progress(self) -> bool:
        if self.state.terminated:
            resp = self.client.Reset(pmodels.GWResetRequest(id=self.id))
            self.state = resp.state

        resp = self.client.DoStep(
            pmodels.GWActionRequest(
                id=self.id,
                drone_actions=
                    [pmodels.GWDroneAction(id=drone.id, action=rand_action()) for drone in self.state.drone_states]
            )
        )
        self.state = resp.state

        print("X:", self.state.drone_states[0].x, "Y:", self.state.drone_states[0].y)
        return any([drone_state.x == 5 and drone_state.y == 5 for drone_state in self.state.drone_states])




def rand_action() -> pmodels.GWAction:
    someRand = random.randint(1, 5);
    if someRand == 1:
        return pmodels.GWAction.NOTHING
    if someRand == 2:
        return pmodels.GWAction.LEFT
    if someRand == 3:
        return pmodels.GWAction.RIGHT
    if someRand == 4:
        return pmodels.GWAction.UP
    if someRand == 5:
        return pmodels.GWAction.DOWN

    return pmodels.GWAction.NOTHING


if __name__ == "__main__":
    run()
