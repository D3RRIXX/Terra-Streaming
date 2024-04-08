using UnityEditor;
using UnityEngine;

namespace TerraStreaming.Modules.SplitTerrain
{
	[CustomEditor(typeof(TerrainSplitterModule))]
	public class TerrainSplitterModuleEditor : TerraModuleEditor<TerrainSplitterModule>
	{
		public override void OnInspectorGUI()
		{
			const string includeTerrains = "_splitTerrainByChunks";
			
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(includeTerrains));
			EditorGUILayout.Space();
			
			if (!Module.SplitTerrainByChunks)
				GUI.enabled = false;
			
			DrawPropertiesExcluding(serializedObject, "m_Script", includeTerrains);
			serializedObject.ApplyModifiedProperties();

			/*if (GUILayout.Button("Split Terrain"))
			{
				Module.SplitTerrain();
			}

			if (GUILayout.Button("Clear Chunks"))
			{
				Module.ClearAllChunks();
			}*/

			GUI.enabled = true;
		}
	}
}