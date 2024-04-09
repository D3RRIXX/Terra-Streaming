using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TerraStreaming
{
	[CustomEditor(typeof(StreamingManager))]
	public class StreamingManagerEditor : Editor
	{
		private SerializedProperty _worldDataProperty;
		private SerializedObject _worldData;

		private void OnEnable()
		{
			_worldDataProperty = serializedObject.FindProperty("_worldData");
			_worldData = new SerializedObject(_worldDataProperty.objectReferenceValue);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_worldData.Update();

			GUI.enabled = false;
			EditorGUILayout.PropertyField(_worldDataProperty);
			GUI.enabled = true;
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_streamingSources"));

			EditorGUILayout.LabelField("Load Ranges", EditorStyles.boldLabel);
			
			SerializedProperty regularLoadRange = _worldData.FindProperty("_loadRange");
			EditorGUILayout.PropertyField(regularLoadRange, new GUIContent("Regular Load Range"));
			
			SerializedProperty impostorLoadRange = _worldData.FindProperty("_impostorLoadRange");
			EditorGUILayout.PropertyField(impostorLoadRange);

			impostorLoadRange.floatValue = Mathf.Max(impostorLoadRange.floatValue, regularLoadRange.floatValue);
			
			serializedObject.ApplyModifiedProperties();
			_worldData.ApplyModifiedProperties();
		}
	}
}