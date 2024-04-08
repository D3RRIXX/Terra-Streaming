using UnityEditor;
using UnityEngine;

namespace TerraStreaming.Modules
{
	public abstract class TerraModuleBase : ScriptableObject
	{
		public abstract string DisplayName { get; }
		public virtual void OnSceneGUI(SceneView sceneView) { }
	}
}