using System.Collections.Generic;
using System.Linq;
using TerraStreaming.Data;
using TerraStreaming.MarkerComponents;
using TerraStreaming.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Modules.CreateImpostors
{
	public class CreateImpostorsModule : TerraModuleBase
	{
		private const string ASSET_GROUP_NAME = "Impostors";

		[SerializeField] private bool _generateImpostors;

		// [SerializeField] private int _terrainResolutionDownscale;
		[SerializeField] private bool _removeImpostorAfterCreation = true;

		public override string DisplayName => "Create Impostors";
		public bool GenerateImpostors => _generateImpostors;

		public void CreateImpostors(WorldData worldData)
		{
			foreach (ChunkData chunkData in worldData.ChunkDataList)
			{
				string savePath = worldData.GetChunkRelatedFolder(chunkData);
				CreatePrefab(chunkData, savePath);
			}
			
			EditorUtility.SetDirty(worldData);
			AssetDatabase.SaveAssets();
		}

		private void CreatePrefab(ChunkData chunkData, string savePath)
		{
			string scenePath = AssetDatabase.GUIDToAssetPath(chunkData.SceneRef.AssetGUID);
			using var sceneScope = OpenSceneScope.Existing(scenePath, _removeImpostorAfterCreation);

			GameObject prefabSource = SetupPrefabSource(sceneScope.Scene, $"{chunkData.Name}_Impostor");

			string prefabPath = CreatePrefabAsset(prefabSource, savePath, out GameObject prefab);

			chunkData.ImpostorPrefab = SetupPrefabReference(chunkData.Name, prefabPath);

			if (!_removeImpostorAfterCreation)
				SpawnImpostorClone(prefab, prefabSource);

			DestroyPrefabSource(prefabSource);

			Debug.Log($"Prefab created at: {AssetDatabase.GetAssetPath(prefab)}");
		}

		private static AssetReferenceGameObject SetupPrefabReference(string groupName, string prefabPath)
		{
			string guid = AddressableUtils.AddAssetToAddressables(groupName, prefabPath);
			return new AssetReferenceGameObject(guid);
		}

		private static GameObject SetupPrefabSource(Scene scene, string name)
		{
			var prefabSource = new GameObject(name);

			GameObject[] rootGameObjects = scene.GetRootGameObjects();

			IEnumerable<ImpostorObjectMarker> impostors = rootGameObjects.SelectMany(x => x.GetComponentsInChildren<ImpostorObjectMarker>());
			foreach (var impostor in impostors)
			{
				GameObject strippedImpostor = impostor.InstantiateStrippedImpostor();
				strippedImpostor.transform.SetParent(prefabSource.transform);
			}

			Terrain originalTerrain = rootGameObjects.Select(x => x.GetComponent<Terrain>()).First();
			Terrain terrainClone = SetupTerrainClone(originalTerrain);
			terrainClone.transform.SetParent(prefabSource.transform);

			// TerrainData cloneTerrainData = terrainClone.terrainData;
			// AssetDatabase.CreateAsset(cloneTerrainData, $"{savePath}/{name}.asset");

			return prefabSource;
		}

		private static Terrain SetupTerrainClone(Terrain originalTerrain)
		{
			// Create a new terrain data object based on the original terrain data
			TerrainData originalTerrainData = originalTerrain.terrainData;
			TerrainData newTerrainData = new()
			{
				// Adjust the heightmap resolution and size of the new terrain
				heightmapResolution = originalTerrainData.heightmapResolution,
				size = originalTerrainData.size
			};

			// Create the new terrain object using the new terrain data
			GameObject newTerrainObject = Terrain.CreateTerrainGameObject(originalTerrainData);
			newTerrainObject.name = originalTerrain.name;

			// Assign the new terrain object to the scene
			newTerrainObject.transform.position = originalTerrain.transform.position;
			newTerrainObject.transform.rotation = originalTerrain.transform.rotation;

			return newTerrainObject.GetComponent<Terrain>();
		}

		private static string CreatePrefabAsset(GameObject prefabSource, string prefabPath, out GameObject prefab)
		{
			prefabPath += $"/{prefabSource.name}.prefab";
			prefab = PrefabUtility.SaveAsPrefabAsset(prefabSource, prefabPath);

			return prefabPath;
		}

		private static void DestroyPrefabSource(GameObject prefabRoot)
		{
			DestroyImmediate(prefabRoot);
		}

		private static void SpawnImpostorClone(GameObject prefab, GameObject prefabRoot)
		{
			var prefabCopy = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			prefabCopy.transform.SetPositionAndRotation(prefabRoot.transform.position, prefabRoot.transform.rotation);
		}
	}
}