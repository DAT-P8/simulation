using System;
using System.Threading.Tasks;
using Godot;
using Simulation.Lib.GW;

namespace Simulation.GridEnvironment
{
	public class GWMap() : IGWWorldGenerator
	{
		// The number of pixels in the tiles' texture 
		private const int tileSize = 16;

        public Task GenerateWorld(GWEnvData envData){
			//Size of map with room for visible borders
            int mapSize = envData.GetMapSize + 2;
            (int,int) target = envData.GetTargetPosition;

            var view = GenerateTexture(mapSize, target);
            Main.MainScene.CallDeferred(Node.MethodName.AddChild, view);
            Main.MainScene.CallDeferred(Node.MethodName.AddChild, ConstructMap(view, mapSize));
            return Task.CompletedTask;
        }

		private static MeshInstance3D ConstructMap(SubViewport mapTexture, int mapSize)
		{
			Vector2I mapDim = new(mapSize, mapSize);
			StandardMaterial3D material = new();
			material.SetTexture(0, mapTexture.GetTexture());

			QuadMesh mesh = new();
			mesh.SetSize(mapDim);
			mesh.ResourceLocalToScene = true;
			mesh.SurfaceSetMaterial(0, material);
			mesh.SetOrientation(PlaneMesh.OrientationEnum.Y);

			MeshInstance3D mesh3d = new();
			mesh3d.SetMesh(mesh);
			return mesh3d;
		}

		private static SubViewport GenerateTexture(int mapSize, (int,int) target)
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
					if ((i,j) == target)
						tileId = new(2,0);
					tileMap.SetCell(coord, 0, tileId, 0);
				}
			}
			mapTexture.AddChild(tileMap);
			return mapTexture;
		}

        /// Converts a picture to a tilemap
		private static TileMapLayer GenerateTilemapLayer()
		{
			Texture2D textureSource = GD.Load<Texture2D>("res://Textures/Simple-tilemap.png");
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
}


// private Func<(float,float),(int,int)> ToTilemapPos => pos => (
//                (int)Math.Ceiling(pos.Item1) + (mapSize/2),
//                (int)Math.Ceiling(pos.Item2) + (mapSize/2)
//        );
// GWEnvData envData = gwFactory.GetEnvData();
// var world = new GWMap(envData);
// var view = world.GenerateTexture();
// AddChild(view);
// AddChild(world.ConstructMap(view));
