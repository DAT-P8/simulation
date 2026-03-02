{
  description = "Simulation environment for PyADRL";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  };

  outputs =
    { nixpkgs, ... }:
    let
      forAllSystems =
        function:
        nixpkgs.lib.genAttrs
          [
            "x86_64-linux"
            "aarch64-linux"
            "x86_64-darwin"
            "aarch64-darwin"
          ]
          (
            system:
            function {
              pkgs = nixpkgs.legacyPackages.${system};
            }
          );
    in
    {
      devShells = forAllSystems (
        { pkgs }:
        let
          dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
        in
        {
          default = pkgs.mkShell {
            packages = [
              dotnet-sdk
              pkgs.godot-mono
              pkgs.protobuf
              pkgs.grpc
              (pkgs.python3.withPackages (ps: [
                ps.grpcio
                ps.grpcio-tools
              ]))
            ];

            # Grpc.Tools NuGet package needs these to find the native binaries on NixOS.
            PROTOBUF_PROTOC = "${pkgs.protobuf}/bin/protoc";
            GRPC_PROTOC_PLUGIN = "${pkgs.grpc}/bin/grpc_csharp_plugin";
            DOTNET_ROOT = "${dotnet-sdk}";
          };
        }
      );
    };
}
