using System.Collections.Generic;
using Godot;
using GW2D.V1;
using Serilog;

namespace Simulation.Services;

public class MapSpawner(ILogger logger, ICameraController cameraController) : IMapSpawner
{
    private readonly Dictionary<RSquareMap, int> _squareMaps = [];
    private readonly ILogger _logger = logger;
    private readonly ICameraController _cameraController = cameraController;
    private readonly PackedScene _greenTile = GD.Load<PackedScene>("res://green_tile.tscn");
    private readonly PackedScene _greyTile = GD.Load<PackedScene>("res://grey_tile.tscn");
    private readonly PackedScene _redTile = GD.Load<PackedScene>("res://red_tile.tscn");

    private readonly List<Node3D> _tiles = [];

    public void SpawnMap(MapSpec mapSpec)
    {
        switch (mapSpec.MapOneofCase)
        {
            case MapSpec.MapOneofOneofCase.SquareMap:
                SpawnSquareMap(mapSpec.SquareMap);
                break;
            default:
                throw new System.Exception($"Did not recognize case of MapSpec: {mapSpec.MapOneofCase}");
        }
    }

    private void SpawnSquareMap(SquareMap squareMap)
    {
        var sqmap = new RSquareMap((int)squareMap.Width, (int)squareMap.Height, (int)squareMap.TargetX, (int)squareMap.TargetY);

        if (!_squareMaps.TryAdd(sqmap, 1))
            _squareMaps[sqmap] += 1;

        if (_squareMaps[sqmap] == 1)
        {
            for (int x = -1; x <= sqmap.Width; x++)
            {
                for (int y = -1; y <= sqmap.Height; y++)
                {
                    Node3D tile;
                    if (x == sqmap.TargetX && y == sqmap.TargetY)
                        tile = _redTile.Instantiate<Node3D>();
                    else if (x == -1 || x == sqmap.Width || y == -1 || y == sqmap.Height)
                        tile = _greyTile.Instantiate<Node3D>();
                    else
                        tile = _greenTile.Instantiate<Node3D>();

                    Main.MainScene.CallDeferred(Node.MethodName.AddChild, tile);
                    _tiles.Add(tile);
                    tile.CallDeferred(Node3D.MethodName.SetPosition, new Vector3I(x, -1, y));
                }
            }

            var midY = sqmap.Height / 2f;
            var midX = sqmap.Width / 2f;

            _cameraController.SetCameraPosition(new(midX, 20, midY));
        }
    }

    // Introduced to ensure value-based equality comparisons in dictionaries.
    private record RSquareMap(int Width, int Height, int TargetX, int TargetY);
}
