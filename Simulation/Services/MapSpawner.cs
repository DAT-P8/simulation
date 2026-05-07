using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GW2D.V1;
using Serilog;
using Simulation.Utils;

namespace Simulation.Services;

public class MapSpawner(ILogger logger, ICameraController cameraController) : IMapSpawner
{
    private readonly ConcurrentDictionary<RSquareMap, int> _squareMaps = [];
    private readonly ILogger _logger = logger;
    private readonly ICameraController _cameraController = cameraController;
    private readonly TileDrawer _tileDrawer = new();

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
        var sqmap = new RSquareMap(
                (int)squareMap.Width,
                (int)squareMap.Height,
                (int)squareMap.TargetX,
                (int)squareMap.TargetY
        );


        if (!_squareMaps.TryAdd(sqmap, 1))
            _squareMaps[sqmap] += 1;

        if (_squareMaps[sqmap] == 1)
        {
            var targetPos = new Position(sqmap.TargetX, sqmap.TargetY);
            var objectPositions = squareMap.Objects.Select(e =>
                {
                    var p = e.GetPosition();
                    return new Position(p.X, p.Y);
                })
                .ToHashSet();

            for (int x = -1; x <= sqmap.Width; x++)
            {
                for (int y = -1; y <= sqmap.Height; y++)
                {
                    var position = new Position(x, y);

                    // Draw tiles here
                    if (position == targetPos)
                        _tileDrawer.Draw(Tile.target, PositionToVec(position));
                    else if (x == -1 || x == sqmap.Width || y == -1 || y == sqmap.Height)
                        _tileDrawer.Draw(Tile.border, PositionToVec(position));
                    else if (objectPositions.Any(e => e == position))
                        _tileDrawer.Draw(Tile.obstacle, PositionToVec(position));
                    else
                        _tileDrawer.Draw(Tile.tile, PositionToVec(position));
                }
            }

            var midY = ((sqmap.Height * 16) + 8) / 2f;
            var midX = ((sqmap.Width * 16) + 8) / 2f;

            _cameraController.SetCameraPosition(new(midX, midY), squareMap);
        }
    }

    private static Vector2I PositionToVec(Position position) => new(position.X, position.Y);

    // Introduced to ensure value-based equality comparisons in dictionaries.
    private record RSquareMap(int Width, int Height, int TargetX, int TargetY);
    private record Position(int X, int Y);
}
