using Godot;

namespace Simulation.GridEnvironment.GridMaps;

public class GWSquareMap()
{
    private static readonly Texture2D textureSource = GD.Load<Texture2D>("res://Textures/Simple-tilemap.png");
    // The number of pixels in the tiles' texture 
    private const int tileSize = 16;

    public static void GenerateWorld(GWBoxEnvData envData)
    {
        //Size of map with room for visible borders
        int mapSize = envData.GetMapSize + 2;
        (int targetX, int targetY) = envData.GetTargetPosition();
        //Convert actual coordinates to the tilemap's coordinates since it has a flipped x-axis
        targetX = mapSize - (targetX + 1);

        var view = GenerateTexture(mapSize, (targetX, targetY));
        Main.MainScene.CallDeferred(Node.MethodName.AddChild, view);
        Main.MainScene.CallDeferred(Node.MethodName.AddChild, ConstructMap(view, mapSize, envData.GetMapPosition()));
    }

    private static MeshInstance3D ConstructMap(SubViewport mapTexture, int mapSize, Vector3 position)
    {
        Vector2I mapDim = new(mapSize, mapSize);
        StandardMaterial3D material = new();
        material.SetTexture(0, mapTexture.GetTexture());
        material.SetTextureFilter(0);

        QuadMesh mesh = new();
        mesh.SetSize(mapDim);
        mesh.ResourceLocalToScene = true;
        mesh.SurfaceSetMaterial(0, material);
        mesh.SetOrientation(PlaneMesh.OrientationEnum.Y);

        MeshInstance3D mesh3d = new();
        mesh3d.SetMesh(mesh);
        mesh3d.SetPosition(position);
        return mesh3d;
    }

    private static SubViewport GenerateTexture(int mapSize, (int, int) target)
    {
        //(map size + 2 borders) * tile size
        Vector2I textureDim = new(mapSize * tileSize, mapSize * tileSize);

        SubViewport mapTexture = new();
        mapTexture.SetSize(textureDim);

        TileMapLayer tileMap = GenerateTilemapLayer();

        //Draw a square map with borders
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                Vector2I coord = new(i, j);
                Vector2I tileId = new(1, 0);
                if (i == 0 || j == 0 || i == (mapSize - 1) || j == (mapSize - 1))
                    tileId = new(0, 0);
                if ((i, j) == target)
                    tileId = new(2, 0);
                tileMap.SetCell(coord, 0, tileId, 0);
            }
        }
        mapTexture.AddChild(tileMap);
        return mapTexture;
    }

    /// Converts a picture to a tilemap
	private static TileMapLayer GenerateTilemapLayer()
    {
        TileSetAtlasSource atlas = new();
        atlas.SetTexture(textureSource);

        Vector2I atlasDim = atlas.GetAtlasGridSize();
        for (int x = 0; x < atlasDim.X; x++)
        {
            for (int y = 0; y < atlasDim.Y; y++)
            {
                Vector2I coord = new(x, y);
                atlas.CreateTile(coord);
            }
        }

        TileSet tileset = new();
        tileset.AddSource(atlas);

        TileMapLayer tilemap = new();
        tilemap.SetTileSet(tileset);
        tilemap.SetTextureFilter(CanvasItem.TextureFilterEnum.Nearest);

        return tilemap;
    }
}
