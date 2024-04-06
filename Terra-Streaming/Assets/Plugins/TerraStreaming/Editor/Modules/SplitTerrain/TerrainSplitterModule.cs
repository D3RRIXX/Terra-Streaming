using System;
using System.Linq;
using TerraStreamer.Data;
using TerraStreamer.Editor.Utilities;
using TerraStreaming.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	public class TerrainSplitterModule : TerraModuleBase
	{
		private const int PADDING = 1;

		[SerializeField] private bool _splitTerrainByChunks;

		[SerializeField] private Terrain _sourceTerrain;
		[SerializeField] private int _chunkHeight = 512;

		[SerializeField, HideInInspector] private Terrain[] _chunks;
		private Terrain[,] _chunkGrid;

		public override string DisplayName => "Split Terrain";
		public bool SplitTerrainByChunks => _splitTerrainByChunks;

		/*public void DrawGizmos()
		{
			if (_sourceTerrain == null)
				return;

			float chunkSize = _sourceTerrain.terrainData.size.x / _chunkAmount.x;
			var size = new Vector3(chunkSize, _chunkHeight, chunkSize);

			Vector3 GetLocalPosition(int x, int z)
			{
				Vector3 pos = Vector3.one * (chunkSize / 2f);
				pos.x += chunkSize * x;
				pos.y = _chunkHeight / 2f;
				pos.z += chunkSize * z;

				return pos;
			}

			Vector2Int chunkAmount = GetChunkAmount();
			for (int x = 0; x < chunkAmount.x; x++)
			for (int y = 0; y < chunkAmount.y; y++)
			{
				Vector3 pos = GetLocalPosition(x, y);

				// Gizmos.color = debugGrid[x, y] == null ? Color.red : Color.magenta;
				// Handles.DrawWireCube(pos, size);
			}
		}*/

		public void AssignTerrainsToChunks(WorldData worldData)
		{
			Terrain[] terrains = SplitTerrain(worldData.GridSettings.GridSize);
			var lookup = terrains.ToLookup(x => Utils.WorldPosToCoords(worldData.GridSettings, x.GetPosition()));

			foreach (ChunkData chunkData in worldData.ChunkDataList)
			{
				string scenePath = AssetDatabase.GUIDToAssetPath(chunkData.SceneRef.AssetGUID);
				using OpenSceneScope sceneScope = SceneUtils.GetOrCreateScene(scenePath, false);

				foreach (Terrain terrain in lookup[chunkData.Coords])
				{
					Utils.MoveObjectToScene(terrain.transform, sceneScope.Scene);

					SaveTerrainDataAsset(chunkData.Name, worldData.GetChunkRelatedFolder(chunkData), terrain.terrainData, out string terrainDataPath);
					AddTerrainDataToAddressables(chunkData.Name, terrainDataPath);
				}
			}
		}

		private Terrain[] SplitTerrain(Vector2Int gridSize)
		{
			ClearAllChunks();

			_sourceTerrain.groupingID = -1;

			TerrainData sourceData = _sourceTerrain.terrainData;

			int chunkResolution = sourceData.heightmapResolution / gridSize.x;
			float chunkSize = sourceData.size.x / gridSize.x;

			_chunkGrid = new Terrain[gridSize.x, gridSize.y];
			_chunks = new Terrain[_chunkGrid.Length];

			for (int x = 0; x < gridSize.x; x++)
			{
				for (int z = 0; z < gridSize.y; z++)
				{
					TerrainData chunkTerrainData = CreateTerrainData(x, z, chunkResolution, sourceData, gridSize);
					Terrain chunk = CreateChunkObject(chunkTerrainData, x, z, chunkSize);

					_chunks[z * gridSize.x + x] = chunk;
					_chunkGrid[x, z] = chunk;

					int startX = x * chunkResolution;
					int startZ = z * chunkResolution;
					float[,] heights = sourceData.GetHeights(startX, startZ, chunkResolution + 1, chunkResolution + 1);

					if (x > 0)
						BlendWithLeftChunk(x, z, heights);

					if (z > 0)
						BlendWithBottomChunk(x, z, heights);

					chunkTerrainData.SetHeights(0, 0, heights);
				}
			}

			_sourceTerrain.gameObject.SetActive(false);
			return _chunks;
		}

		private static void SaveTerrainDataAsset(string chunkName, string chunkFolder, TerrainData terrainData, out string terrainDataPath)
		{
			if (AssetDatabase.IsMainAsset(terrainData))
			{
				terrainDataPath = null;
				return;
			}
				
			terrainDataPath = $"{chunkFolder}/{chunkName}_TerrainData.asset";
			AssetDatabase.CreateAsset(terrainData, terrainDataPath);
		}

		private static void AddTerrainDataToAddressables(string groupName, string terrainDataPath)
		{
			AddressableUtils.AddAssetToAddressables(groupName, terrainDataPath);
		}

		private Terrain CreateChunkObject(TerrainData terrainData, int x, int z, float chunkSize)
		{
			var chunk = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
			chunk.name = terrainData.name;
			chunk.transform.localPosition = new Vector3(x * chunkSize, 0, z * chunkSize);

			chunk.materialTemplate = _sourceTerrain.materialTemplate;
			return chunk;
		}

		private TerrainData CreateTerrainData(int x, int z, int chunkResolution, TerrainData sourceData, Vector2Int gridSize)
		{
			var chunkTerrainData = new TerrainData
			{
				name = $"Terrain_{x}_{z}",
				heightmapResolution = chunkResolution + 1,
				baseMapResolution = sourceData.baseMapResolution,
				alphamapResolution = sourceData.alphamapResolution,
				size = new Vector3(sourceData.size.x / gridSize.x, sourceData.size.y, sourceData.size.z / gridSize.y)
			};

			chunkTerrainData.SetDetailResolution(sourceData.detailResolution, sourceData.detailResolutionPerPatch);

			return chunkTerrainData;
		}

		public void ClearAllChunks()
		{
			// foreach (Terrain chunk in _chunks)
			// {
			// 	DestroyImmediate(chunk.gameObject);
			// }
			//
			// _chunks = Array.Empty<Terrain>();
		}

		private void BlendWithLeftChunk(int x, int z, float[,] heights)
		{
			Terrain leftTerrain = _chunkGrid[x - 1, z];
			int heightmapResolution = leftTerrain.terrainData.heightmapResolution;

			float[,] leftHeights = leftTerrain.terrainData.GetHeights(heightmapResolution - PADDING, 0, PADDING,
				heightmapResolution);

			for (int i = 0; i < PADDING; i++)
			{
				float t = i / (float)PADDING;
				for (int j = 0; j < heights.GetLength(0); j++)
				{
					heights[j, i] = Mathf.Lerp(leftHeights[j, leftHeights.GetLength(1) - PADDING + i], heights[j, i], t);
				}
			}
		}

		private void BlendWithBottomChunk(int x, int z, float[,] heights)
		{
			Terrain bottomTerrain = _chunkGrid[x, z - 1];

			int heightmapResolution = bottomTerrain.terrainData.heightmapResolution;

			float[,] bottomHeights = bottomTerrain.terrainData.GetHeights(0, heightmapResolution - PADDING,
				heightmapResolution, PADDING);

			for (int j = 0; j < PADDING; j++)
			{
				float t = (float)j / PADDING;
				for (int i = 0; i < heights.GetLength(0); i++)
				{
					heights[j, i] = Mathf.Lerp(bottomHeights[bottomHeights.GetLength(0) - PADDING + j, i],
						heights[j, i], t);
				}
			}
		}
	}
}