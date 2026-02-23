import grid_world_pb2 as proto_models
import grid_world_pb2_grpc as grpc_models
import grpc

class IdCounter:
    id = 0

    @staticmethod
    def NewId() -> int:
        IdCounter.id += 1
        return IdCounter.id

def run():

    with grpc.insecure_channel("localhost:50051") as channel:
        stub = grpc_models.GWSimulationStub(channel)

        id = IdCounter.NewId()
        response: proto_models.GWResetResponse = stub.Reset(proto_models.GWResetRequest(id=id))
        print(f"Received: {response}")

        defender_states = response.state.defender_drone_states
        evader_states = response.state.evader_drone_states
        terminated = response.state.terminated

        if (terminated):
            return

        defender_actions = [proto_models.GWDroneAction(id=d.id, action=proto_models.GWAction.LEFT) for d in defender_states]
        evader_actions = [proto_models.GWDroneAction(id=d.id, action=proto_models.GWAction.LEFT) for d in evader_states]
        actions = defender_actions + evader_actions

        response = stub.DoStep(proto_models.GWActionRequest(id=id, actions=actions))

if __name__ == "__main__":
    run()
