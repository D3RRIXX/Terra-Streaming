using UnityEditor;
using UnityEngine;

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

		private void OnDisable()
		{
			_worldData.Dispose();
			_worldData = null;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_worldData.Update();

			GUI.enabled = false;
			
			EditorGUILayout.PropertyField(_worldDataProperty);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_streamingSources"));
			
			GUI.enabled = true;

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