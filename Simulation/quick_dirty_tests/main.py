import grid_world_pb2 as proto_models
import random
import grid_world_pb2_grpc as grpc_models
import grpc

def run():

    with grpc.insecure_channel("localhost:50051") as channel:
        stub = grpc_models.GWSimulationStub(channel)

        while True:
            response: proto_models.GWNewResponse = stub.New(proto_models.GWNewRequest())
            id = response.id;

            while True:
                drone_states = response.state.drone_states
                actions = [proto_models.GWDroneAction(id=d.id, action=rand_action()) for d in drone_states]
                response = stub.DoStep(proto_models.GWActionRequest(id=response.id, drone_actions=actions))

                if response.state.terminated:
                    stub.Close(proto_models.GWCloseRequest(id=id));
                    break;

def rand_action() -> proto_models.GWAction:
    someRand = random.randint(1, 5);
    if someRand == 1:
        return proto_models.GWAction.NOTHING
    if someRand == 2:
        return proto_models.GWAction.LEFT
    if someRand == 3:
        return proto_models.GWAction.RIGHT
    if someRand == 4:
        return proto_models.GWAction.UP
    if someRand == 5:
        return proto_models.GWAction.DOWN

    return proto_models.GWAction.NOTHING


if __name__ == "__main__":
    run()
