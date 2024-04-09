using System.Collections.Generic;
using System.Linq;
using TerraStreaming.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreaming
{
	[ExecuteInEditMode]
	public class EditorChunkLoader : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] private WorldData _worldData;

		private List<Scene> _openScenes;

		public WorldData WorldData => _worldData;

		private void LoadAllChunks()
		{
			_openScenes = _worldData.ChunkDataList
				.Select(chunkData => AssetDatabase.GetAssetPath(chunkData.SceneRef.editorAsset))
				.Select(scenePath => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive)).ToList();
		}

		private void UnloadAllChunks()
		{
			foreach (Scene scene in _openScenes)
			{
				EditorSceneManager.CloseScene(scene, true);
			}
		}
#endif
	}
}