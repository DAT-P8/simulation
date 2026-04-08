#!/usr/bin/env bash

set -e

cd "$(dirname $0)"

python -m grpc_tools.protoc -I../../Protos --python_out=. --pyi_out=. --grpc_python_out=. ../../Protos/TDF.proto

python -m grpc_tools.protoc -I../../Protos --python_out=. --pyi_out=. --grpc_python_out=. ../../Protos/ngw/v1/ngw2d.proto
