using System;
using System.Linq;
using TerraStreamer.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TerraStreaming.Editor
{
	public static class Utils
	{
		public static string ConvertAbsolutePathToRelative(string absolutePath) => "Assets" + absolutePath[Application.dataPath.Length..];

		public static string GetOrCreateFolder(string parentFolder, string folderName)
		{
			string combinedPath = $"{parentFolder}/{folderName}";
			if (AssetDatabase.IsValidFolder(combinedPath))
				return combinedPath;

			string guid = AssetDatabase.CreateFolder(parentFolder, folderName);
			return string.IsNullOrEmpty(guid) ? string.Empty : AssetDatabase.GUIDToAssetPath(guid);
		}

		public static T GetOrCreateAsset<T>(string parentFolder, string assetName, Func<T> factory) where T : Object
		{
			string combinedPath = $"{parentFolder}/{assetName}";
			if (AssetExists(combinedPath, out Object asset))
				return (T)asset;

			T concreteAsset = factory();
			AssetDatabase.CreateAsset(concreteAsset, combinedPath);
			return concreteAsset;
		}

		public static Vector2Int WorldPosToCoords(GridSettings gridSettings, Vector3 position)
		{
			float gridSize = gridSettings.CellSize;
			// position -= CalculateCenter(gridSettings);

			return new Vector2Int(Mathf.FloorToInt(position.x / gridSize), Mathf.FloorToInt(position.z / gridSize));
		}
		
		public static Vector3 CellPosition(GridSettings gridSettings, int x, int y)
		{
			Vector3 centerOffset = CalculateCenter(gridSettings);
			Vector3 pos = new(x * gridSettings.CellSize, 0f, y * gridSettings.CellSize);

			return pos + centerOffset + gridSettings.CenterOffset;
		}

		public static Vector3 CalculateCenter(GridSettings gridSettings)
		{
			Vector3 centerOffset = Vector3.one * (gridSettings.CellSize / 2f);
			centerOffset.y = 0f;
			return centerOffset;
		}

		public static bool AssetExists(string path, out Object asset)
		{
			asset = AssetDatabase.LoadMainAssetAtPath(path);
			return asset != null;
		}

		public static string GetChunkRelatedFolder(this WorldData worldData, ChunkData chunkData)
		{
			if (!worldData.ChunkDataList.Contains(chunkData))
				throw new InvalidOperationException();

			string assetPath = AssetDatabase.GetAssetPath(worldData);
			return $"{SceneUtils.GetParentFolder(assetPath)}/{chunkData.Name}";
		}

		public static void MoveObjectToScene(Transform transform, in Scene scene)
		{
			const string actionName = "Terra (Move GameObject to scene)";

			Undo.SetCurrentGroupName(actionName);
			Undo.SetTransformParent(transform, newParent: null, actionName);
			Undo.MoveGameObjectToScene(transform.gameObject, scene, actionName);
		}
	}
}