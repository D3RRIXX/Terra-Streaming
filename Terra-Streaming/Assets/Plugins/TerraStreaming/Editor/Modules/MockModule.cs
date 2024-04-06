using UnityEditor;
using UnityEngine;

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	public class MockModule : ScriptableObject
	{
		[CustomEditor(typeof(MockModule))]
		private class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				EditorGUILayout.LabelField("This is a mock module. Replace me senpai!");
			}
		}
	}
}