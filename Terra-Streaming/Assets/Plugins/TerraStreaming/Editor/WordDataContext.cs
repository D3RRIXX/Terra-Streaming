using TerraStreaming.Data;
using TerraStreaming.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreaming
{
	public static class WordDataContext
	{
		private static WorldData worldData;

		public static WorldData WorldData => worldData ??= GetActualWorldData();

		public static WorldData CreateTempWorldData(string worldName)
		{
			worldData = ScriptableObject.CreateInstance<WorldData>();
			worldData.name = worldName;

			return worldData;
		}
		
		private static WorldData GetActualWorldData()
		{
			// 1. Look for Streaming Manager
			// 2. Look for World Data in folder

			var streamingManager = StreamingManager.Instance;
			if (streamingManager != null)
			{
				if (streamingManager.WorldData != null)
					return streamingManager.WorldData;
				
				Debug.LogWarning($"Scene {streamingManager.gameObject.scene.name} contains Streaming Manager with no World Data!", streamingManager);
			}

			var activeScene = SceneManager.GetActiveScene();
			return activeScene.GetAssociatedWorldData();
		}
	}
}