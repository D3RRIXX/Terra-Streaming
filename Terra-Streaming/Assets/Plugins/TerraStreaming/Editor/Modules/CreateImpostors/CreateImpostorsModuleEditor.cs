using UnityEditor;
using UnityEngine;

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	[CustomEditor(typeof(CreateImpostorsModule))]
	public class CreateImpostorsModuleEditor : TerraModuleEditor<CreateImpostorsModule>
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty property = serializedObject.FindProperty("_generateImpostors");
			EditorGUILayout.PropertyField(property);
			
			EditorGUILayout.Space();
			
			if (!property.boolValue)
				GUI.enabled = false;
			
			serializedObject.ApplyModifiedProperties();
			
			// EditorGUILayout.PropertyField(serializedObject.FindProperty("_worldData"), new GUIContent("Source World Data"));
			// EditorGUILayout.PropertyField(serializedObject.FindProperty("_removeImpostorAfterCreation"));
			//
			// if (!ShouldDisplayCreateButton())
			// {
			// 	EditorGUILayout.HelpBox("Assign a Location Data!", MessageType.Info);
			// 	GUI.enabled = true;
			// 	return;
			// }
			//
			// EditorGUILayout.Space();
			//
			// if (GUILayout.Button("Create Impostors"))
			// {
			// 	Module.CreateImpostors();
			// }

			GUI.enabled = true;
		}
	}
}