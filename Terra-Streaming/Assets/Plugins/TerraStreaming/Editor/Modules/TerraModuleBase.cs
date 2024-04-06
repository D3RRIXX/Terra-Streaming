using UnityEditor;
using UnityEngine;

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	public abstract class TerraModuleBase : ScriptableObject
	{
		public abstract string DisplayName { get; }
		public virtual void OnSceneGUI(SceneView sceneView) { }
	}
}