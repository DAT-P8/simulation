using System;
using Godot;

namespace Simulation.Utils;

public enum Tile
{
    tile,
    border,
    obstacle,
    target
}

public class TileDrawer
{
    private readonly TileMapLayer _tileMap = SceneObjectGetter.GetObject<TileMapLayer>();

    public void Draw(Tile tile, Vector2I position)
    {
        Vector2I atlasCord = GetAtlasCord(tile);
        _tileMap.SetCell(position, 1, atlasCord, 0);

    }

    private static Vector2I GetAtlasCord(Tile tile) => tile switch
    {
        Tile.tile => new Vector2I(0, 0),
        Tile.obstacle => new Vector2I(1, 0),
        Tile.border => new Vector2I(2, 0),
        Tile.target => new Vector2I(3, 0),
        _ => throw new Exception("Recieved unknown tile")
    };
}
