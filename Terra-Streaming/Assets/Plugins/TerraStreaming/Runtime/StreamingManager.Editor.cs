using System.Collections.Generic;
using System.Linq;
using TerraStreaming.Data;
using TerraStreaming.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AddressableAssets;
#endif

namespace TerraStreaming
{
	public partial class StreamingManager
	{
#if UNITY_EDITOR
		private List<Scene> _openScenes;

		/// <summary>
		/// EDITOR ONLY
		/// </summary>
		public void LoadAllChunks()
		{
			_openScenes = _worldData.ChunkDataList
			                        .Select(x => x.SceneRef)
			                        .Select(AssetRefToPath)
			                        .Select(LoadScene)
			                        .ToList();
		}

		/// <summary>
		/// EDITOR ONLY
		/// </summary>
		public void UnloadAllChunks()
		{
			foreach (Scene scene in _openScenes)
			{
				EditorSceneManager.CloseScene(scene, true);
			}
		}

		public void SetChunkEnabled(int x, int y, bool value)
		{
			if (value)
				LoadChunk(x, y);
			else
				UnloadChunk(x, y);
		}

		private void LoadChunk(int x, int y)
		{
			ChunkData chunkData = _worldData.GetChunkAt(x, y);
			string scenePath = AssetRefToPath(chunkData.SceneRef);
			LoadScene(scenePath);
		}

		private void UnloadChunk(int x, int y)
		{
			ChunkData chunkData = _worldData.GetChunkAt(x, y);
			string scenePath = AssetRefToPath(chunkData.SceneRef);

			IEnumerable<Scene> scenes = Enumerable.Range(0, SceneManager.sceneCount)
			                                      .Select(EditorSceneManager.GetSceneAt);
			foreach (Scene scene in scenes)
			{
				if (string.Equals(scenePath, scene.path))
				{
					EditorSceneManager.CloseScene(scene, true);
					break;
				}
			}
		}

		private static Scene LoadScene(string scenePath) => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
		private static string AssetRefToPath(AssetReference assetReference) => AssetDatabase.GetAssetPath(assetReference.editorAsset);
#endif
	}
}