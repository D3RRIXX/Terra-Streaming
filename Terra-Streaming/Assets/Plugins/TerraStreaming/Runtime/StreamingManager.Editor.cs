using System.Collections.Generic;
using System.Linq;
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
			                        .Select(scenePath => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive))
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

		private static string AssetRefToPath(AssetReference assetReference) => AssetDatabase.GetAssetPath(assetReference.editorAsset);
#endif
	}
}