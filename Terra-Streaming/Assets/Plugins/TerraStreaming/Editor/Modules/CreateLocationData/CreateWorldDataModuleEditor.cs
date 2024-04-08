using UnityEditor;
using UnityEngine;

namespace TerraStreaming.Modules.CreateLocationData
{
	[CustomEditor(typeof(CreateWorldDataModule))]
	public class CreateWorldDataModuleEditor : TerraModuleEditor<CreateWorldDataModule>
	{
		public override void OnInspectorGUI()
		{
			DrawLocationName();

			serializedObject.Update();
			SerializedProperty overrideName = serializedObject.FindProperty("_overrideLocationName");

			EditorGUILayout.PropertyField(overrideName);
			if (overrideName.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_locationName"));

			DrawAsBox(serializedObject.FindProperty("_gridSettings"));
			DrawAsBox(serializedObject.FindProperty("_groupingSettings"));

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();

			/*if (GUILayout.Button("Create Location Data"))
			{
				Module.SetupTerrainData();
			}*/
		}

		private void DrawLocationName()
		{
			var locationName = Module.GetLocationName();
			if (string.IsNullOrEmpty(locationName))
				EditorGUILayout.HelpBox("Can't have empty World name!", MessageType.Error);
			else
				EditorGUILayout.HelpBox($"World name will be {locationName}", MessageType.Info, true);
		}

		private static void DrawAsBox(SerializedProperty property)
		{
			GUILayout.BeginVertical(property.displayName, GUI.skin.box);
			EditorGUILayout.PropertyField(property, GUIContent.none);
			GUILayout.EndVertical();
		}
	}
}