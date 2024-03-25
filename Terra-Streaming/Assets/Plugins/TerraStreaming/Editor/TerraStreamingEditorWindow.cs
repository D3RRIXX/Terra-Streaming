using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Editor.Plugins.TerraStreaming.Editor
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

		[SerializeField] private GridSettings _gridSettings = new();

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
			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button("Generate Scenes"))
			{
				var groups = GroupObjectsByChunks().ToList();
			}
		}

		private IEnumerable<IGrouping<Vector2Int, GameObject>> GroupObjectsByChunks()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			var objects = activeScene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>()).ToList();
			
			foreach (Transform t in objects)
			{
				Vector2Int coords = WorldPosToCoords(_gridSettings, t.position);
				Debug.Log($"{t.name} is at cell {coords}", t);
			}

			return objects.Select(x => x.gameObject).GroupBy(x => WorldPosToCoords(_gridSettings, x.transform.position));
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