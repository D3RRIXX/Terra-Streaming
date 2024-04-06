using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TerraStreamer.Data
{
	[Serializable]
	public class ChunkData
	{
		[SerializeField, HideInInspector] private string _name;
		[SerializeField, SerializeReference] private ChunkData _leftNeighbour;
		[SerializeField, SerializeReference] private ChunkData _rightNeighbour;
		[SerializeField, SerializeReference] private ChunkData _topNeighbour;
		[SerializeField, SerializeReference] private ChunkData _bottomNeighbour;
		
		public AssetReferenceGameObject ImpostorPrefab;
		public AssetReference SceneRef;
		public Vector2Int Coords;
		[HideInInspector] public Bounds Bounds;

		public string Name => _name;

		public ChunkData LeftNeighbour
		{
			get => _leftNeighbour;
			set
			{
				_leftNeighbour = value;
				if (value != null)
					value._rightNeighbour = this;
			}
		}

		public ChunkData RightNeighbour
		{
			get => _rightNeighbour;
			set
			{
				_rightNeighbour = value;
				if (value != null)
					value._leftNeighbour = this;
			}
		}

		public ChunkData TopNeighbour
		{
			get => _topNeighbour;
			set
			{
				_topNeighbour = value;
				if (value != null)
					value._bottomNeighbour = this;
			}
		}

		public ChunkData BottomNeighbour
		{
			get => _bottomNeighbour;
			set
			{
				_bottomNeighbour = value;
				if (value != null)
					value._topNeighbour = this;
			}
		}

		public ChunkData(string name)
		{
			_name = name;
		}
		
		public ChunkData()
		{
		}

#if UNITY_EDITOR
		public void SetupBounds()
		{
			string scenePath = GetScenePath();
			var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

			Terrain terrain = null;
			foreach (GameObject gameObject in scene.GetRootGameObjects())
			{
				if (gameObject.TryGetComponent(out terrain))
					break;
			}

			if (terrain == null)
				throw new NullReferenceException($"No terrain found in scene '{_name}'");

			Bounds bounds = terrain.terrainData.bounds;
			bounds.center += terrain.GetPosition();
			Bounds = bounds;

			EditorSceneManager.CloseScene(scene, true);

			Debug.Log($"Setup bounds for {_name}!");
		}

		private string GetScenePath()
		{
#if ADDRESSABLES_ENABLED
			return AssetDatabase.GetAssetPath(SceneRef.editorAsset);
#else
			return AssetDatabase.GetAssetPath(ScenePath);
#endif
		}
		
#endif
	}
}