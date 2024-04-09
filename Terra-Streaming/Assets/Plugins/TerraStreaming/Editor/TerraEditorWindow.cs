using System.Linq;
using TerraStreaming.Modules;
using TerraStreaming.Modules.CreateImpostors;
using TerraStreaming.Modules.CreateLocationData;
using TerraStreaming.Modules.SplitTerrain;
using UnityEditor;
using UnityEngine;

namespace TerraStreaming
{
	public class TerraEditorWindow : EditorWindow
	{
		private TerraModuleBase[] _modules;
		private Editor _activeEditor;

		private int _selectedTab;
		private Vector2 _scrollPos;

		private TerraModuleBase[] Modules => _modules ??= new TerraModuleBase[]
		{
			CreateInstance<CreateWorldDataModule>(),
			CreateInstance<TerrainSplitterModule>(),
			CreateInstance<CreateImpostorsModule>()
		};

		[MenuItem("Window/Terra Streaming/World Editor")]
		private static void ShowWindow()
		{
			var window = GetWindow<TerraEditorWindow>();
			window.titleContent = new GUIContent("World Editor");
			window.Show();
		}

		private void OnEnable()
		{
			SceneView.duringSceneGui += OnSceneGUI;
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
		}

		private void OnSceneGUI(SceneView obj)
		{
			// TODO: Replace this with some shared WorldData context for every scene
			Modules[0].OnSceneGUI(obj);
		}

		private void OnGUI()
		{
			DrawToolbar();

			if (_activeEditor is null)
				CreateEditor(_selectedTab);

			DrawActiveEditor();

			if (GUILayout.Button("Generate World", GUILayout.MinHeight(60)))
				GenerateWorld();
		}

		private void GenerateWorld()
		{
			var createWorldData = GetModule<CreateWorldDataModule>();
			var worldData = createWorldData.SetupWorldData();

			var terrainSplitter = GetModule<TerrainSplitterModule>();
			if (terrainSplitter.SplitTerrainByChunks)
				terrainSplitter.AssignTerrainsToChunks(worldData);

			var impostorsModule = GetModule<CreateImpostorsModule>();
			if (impostorsModule.GenerateImpostors)
				impostorsModule.CreateImpostors(worldData);
		}

		private void DrawActiveEditor()
		{
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

			_activeEditor.OnInspectorGUI();
			EditorGUILayout.EndScrollView();
		}

		private T GetModule<T>() where T : TerraModuleBase => Modules.OfType<T>().Single();

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			for (int i = 0; i < Modules.Length; i++)
			{
				bool isTabSelected = DrawToolbarButton(i, Modules[i].DisplayName, out string controlName);

				if (!isTabSelected || _selectedTab == i)
					continue;

				if (_selectedTab != i)
					SwitchEditorTo(i);

				GUI.FocusControl(controlName);
			}

			GUILayout.EndHorizontal();
		}

		public void SwitchEditorTo(int i)
		{
			DestroyActiveEditor();
			CreateEditor(i);
			_selectedTab = i;
		}

		private bool DrawToolbarButton(int i, string tabName, out string controlName)
		{
			controlName = $"TabButton{i}";

			GUI.SetNextControlName(controlName);
			bool isTabSelected = GUILayout.Toggle(_selectedTab == i, tabName, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
			return isTabSelected;
		}

		private void DestroyActiveEditor()
		{
			if (_activeEditor != null)
				DestroyImmediate(_activeEditor);
		}

		private void CreateEditor(int i)
		{
			_activeEditor = Editor.CreateEditor(Modules[i]);
		}
	}
}