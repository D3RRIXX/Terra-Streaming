using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Editor
{
	public class TerraStreamingEditorWindow : EditorWindow
	{
		[MenuItem("Window/Terra Streaming/World Editor")]
		private static void ShowWindow()
		{
			var window = GetWindow<TerraStreamingEditorWindow>();
			window.titleContent = new GUIContent("World Editor");
			window.Show();
		}

		[SerializeField] private GridSettings _gridSettings;
		[SerializeField] private ObjectGroupingSettings _groupingSettings;

		private SerializedObject _serializedObject;
		private SerializedObject SerializedObject => _serializedObject ??= new SerializedObject(this);

		private void OnEnable()
		{
			SceneView.duringSceneGui += OnSceneGUI;
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
		}

		private void OnGUI()
		{
			SerializedObject serializedObject = SerializedObject;

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridSettings"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_groupingSettings"));
			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button("Generate Scenes"))
			{
				GenerateScenes();
			}
		}

		private void GenerateScenes()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			string targetPath = SceneUtils.GetOrCreateFolder(activeScene.GetParentFolder(), activeScene.name);

			var children = _groupingSettings.Parents.SelectMany(GroupChildrenByChunks);
			var objects = GroupObjectsByChunks(_groupingSettings.IndividualObjects);
			
			var parent = _groupingSettings.Parents[0];
			foreach ((Vector2Int coords, Transform chunkParent) in GroupChildrenByChunks(parent))
			{
				string sceneName = $"{activeScene.name}_{coords}";
				using OpenSceneScope sceneScope = SceneUtils.GetOrCreateScene($"{targetPath}/{sceneName}.unity", closeOnDispose: false);
				MoveObjectToScene(chunkParent, sceneScope.Scene);
			}

			EditorSceneManager.MarkSceneDirty(activeScene);
			SceneManager.SetActiveScene(activeScene);
		}

		private static void MoveObjectToScene(Transform transform, in Scene scene)
		{
			const string actionName = "Terra (Move GameObject to scene)";
			
			Undo.SetCurrentGroupName(actionName);
			Undo.SetTransformParent(transform, newParent: null, actionName);
			Undo.MoveGameObjectToScene(transform.gameObject, scene, actionName);
		}

		private IEnumerable<(Vector2Int, Transform)> GroupChildrenByChunks(Transform parent)
		{
			var objects = parent.GetComponentsInChildren<Transform>().Skip(1);
			foreach (IGrouping<Vector2Int,Transform> grouping in objects.GroupBy(x => WorldPosToCoords(_gridSettings, x.position)))
			{
				Transform chunkParent = new GameObject(parent.name).transform;
				foreach (Transform child in grouping)
				{
					child.SetParent(chunkParent, true);
				}

				yield return (grouping.Key, chunkParent);
			}
		}

		private IEnumerable<IGrouping<Vector2Int, Transform>> GroupObjectsByChunks(IEnumerable<Transform> transforms)
		{
			return
				from t in transforms
				group t by WorldPosToCoords(_gridSettings, t.position)
				into grouping
				select grouping;
		}

		private void OnSceneGUI(SceneView obj)
		{
			var size = new Vector3(_gridSettings.CellSize, 0f, _gridSettings.CellSize);

			for (int x = 0; x < _gridSettings.GridSize.x; x++)
			for (int y = 0; y < _gridSettings.GridSize.y; y++)
			{
				Handles.DrawWireCube(CellPosition(_gridSettings, x, y), size);
			}
		}

		private static Vector3 CellPosition(GridSettings gridSettings, int x, int y)
		{
			Vector3 centerOffset = CalculateCenter(gridSettings);
			Vector3 pos = new(x * gridSettings.CellSize, 0f, y * gridSettings.CellSize);

			return pos + centerOffset + gridSettings.CenterOffset;
		}

		private static Vector3 CalculateCenter(GridSettings gridSettings)
		{
			Vector3 centerOffset = Vector3.one * (gridSettings.CellSize / 2f);
			centerOffset.y = 0f;
			return centerOffset;
		}

		private static Vector2Int WorldPosToCoords(GridSettings gridSettings, Vector3 position)
		{
			float gridSize = gridSettings.CellSize;
			// position -= CalculateCenter(gridSettings);

			return new Vector2Int(Mathf.FloorToInt(position.x / gridSize), Mathf.FloorToInt(position.z / gridSize));
		}
	}
}