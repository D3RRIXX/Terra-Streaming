using System;
using System.Collections.Generic;
using System.Linq;
using TerraStreamer.Data;
using TerraStreamer.Editor;
using TerraStreamer.Editor.Utilities;
using TerraStreaming.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

// ReSharper disable Unity.InefficientMultidimensionalArrayUsage

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	public class CreateWorldDataModule : TerraModuleBase
	{
		[SerializeField] private string _locationName;
		[SerializeField] private bool _overrideLocationName;
		[SerializeField] private GridSettings _gridSettings;
		[SerializeField] private ObjectGroupingSettings _groupingSettings;

		private WorldData _worldData;

		public override string DisplayName => "World Settings";
		public GridSettings GridSettings => _gridSettings;

		public string GetLocationName() => _overrideLocationName ? _locationName : SceneManager.GetActiveScene().name;

		public WorldData SetupWorldData()
		{
			try
			{
				Vector2Int gridSize = _gridSettings.GridSize;
				Scene activeScene = SceneManager.GetActiveScene();
				int length = gridSize.x * gridSize.y;

				string parentFolder = Utils.GetOrCreateFolder(SceneUtils.GetParentFolder(activeScene.path), activeScene.name);
				_worldData = GetWorldDataAsset(parentFolder, GetLocationName());
				_worldData.GridSettings = _gridSettings;

				Dictionary<Vector2Int, List<Transform>> objectMap = GetAllObjectsToSort(_groupingSettings).ToDictionary(x => x.Key, x => x.Objects);

				int progress = 0;

				var chunkGrid = new ChunkData[gridSize.x, gridSize.y];

				for (int x = 0; x < gridSize.x; x++)
				for (int y = 0; y < gridSize.y; y++)
				{
					UpdateProgressBar(progress, length);

					var coords = new Vector2Int(x, y);
					string chunkName = $"{activeScene.name}_{coords}";
					string chunkFolderPath = Utils.GetOrCreateFolder(parentFolder, chunkName);
					string scenePath = $"{chunkFolderPath}/{chunkName}.unity";

					using OpenSceneScope sceneScope = SceneUtils.GetOrCreateScene(scenePath, closeOnDispose: false);

					ChunkData chunkData = CreateChunkData(scenePath, chunkName, coords);

					if (x > 0)
						chunkData.LeftNeighbour = chunkGrid[x - 1, y];

					if (y > 0)
						chunkData.BottomNeighbour = chunkGrid[x, y - 1];

					if (objectMap.TryGetValue(coords, out List<Transform> transforms))
					{
						foreach (Transform transform in transforms)
						{
							Utils.MoveObjectToScene(transform, sceneScope.Scene);
						}

						EditorSceneManager.MarkSceneDirty(activeScene);
						SceneManager.SetActiveScene(activeScene);
					}

					_worldData.AddChunkData(chunkData);
					progress++;
				}

				SceneManager.SetActiveScene(activeScene);

				// TODO: Restore
				// _worldData.UpdateChunkBounds();

				EditorUtility.SetDirty(_worldData);
				AssetDatabase.SaveAssets();
			}
			finally
			{
				// _terrains = Array.Empty<Terrain>();
				EditorUtility.ClearProgressBar();
			}

			return _worldData;
		}

		public override void OnSceneGUI(SceneView sceneView)
		{
			var size = new Vector3(_gridSettings.CellSize, 0f, _gridSettings.CellSize);

			for (int x = 0; x < _gridSettings.GridSize.x; x++)
			for (int y = 0; y < _gridSettings.GridSize.y; y++)
			{
				Handles.DrawWireCube(Utils.CellPosition(_gridSettings, x, y), size);
			}
		}

		private static void UpdateProgressBar(int progress, int length)
		{
			EditorUtility.DisplayProgressBar("Create Terrain Scenes", $"Creating Scene (Step {progress + 1}/{length})",
				length / (float)progress);
		}

		private static ChunkData CreateChunkData(string scenePath, string chunkName, Vector2Int coords)
		{
			return new ChunkData(chunkName)
			{
				SceneRef = GetChunkSceneReference(chunkName, scenePath),
				Coords = coords
			};
		}

		private IEnumerable<(Vector2Int Key, List<Transform> Objects)> GetAllObjectsToSort(ObjectGroupingSettings groupingSettings)
		{
			IEnumerable<IGrouping<Vector2Int, Transform>> objects = GroupObjectsByChunks(groupingSettings.IndividualObjects);
			IEnumerable<IGrouping<Vector2Int, Transform>> parents = groupingSettings.Parents
			                                                                        .SelectMany(GroupChildrenByChunks)
			                                                                        .GroupBy(x => x.Item1, x => x.Item2)
			                                                                        .ToList();

			// TODO: Fix this
			return parents.Select(x => (x.Key, new List<Transform>(x)));

			return objects.Join(parents, grouping => grouping.Key, grouping => grouping.Key,
				(grouping1, grouping2) => (grouping1.Key, new List<Transform>(grouping1.Concat(grouping2))));
		}

		private IEnumerable<(Vector2Int, Transform)> GroupChildrenByChunks(Transform parent)
		{
			var objects = parent.GetComponentsInChildren<Transform>().Skip(1);
			foreach (IGrouping<Vector2Int, Transform> grouping in objects.GroupBy(x => Utils.WorldPosToCoords(_gridSettings, x.position)))
			{
				Transform chunkParent = new GameObject(parent.name).transform;
				foreach (Transform child in grouping)
				{
					child.SetParent(chunkParent, true);
				}

				ChunkObjectMarker.AddTo(chunkParent.gameObject, ObjectCollectionType.CollectionParent);
				yield return (grouping.Key, chunkParent);
			}

			Undo.DestroyObjectImmediate(parent.gameObject);
		}

		private IEnumerable<IGrouping<Vector2Int, Transform>> GroupObjectsByChunks(IEnumerable<Transform> transforms)
		{
			foreach (IGrouping<Vector2Int, Transform> grouping in transforms.GroupBy(t => Utils.WorldPosToCoords(_gridSettings, t.position)))
			{
				foreach (Transform transform in grouping)
				{
					ChunkObjectMarker.AddTo(transform.gameObject, ObjectCollectionType.SingleObject);
				}

				yield return grouping;
			}
		}

		private static WorldData GetWorldDataAsset(string parentFolder, string worldName) =>
			Utils.GetOrCreateAsset(parentFolder, $"{worldName}_WorldData.asset", CreateInstance<WorldData>);

		private static AssetReference GetChunkSceneReference(string groupName, string scenePath)
		{
			string guid = AddressableUtils.AddAssetToAddressables(groupName, scenePath);
			return new AssetReference(guid);
		}
	}
}