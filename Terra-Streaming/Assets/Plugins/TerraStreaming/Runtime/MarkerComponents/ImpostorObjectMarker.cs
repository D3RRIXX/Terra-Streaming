using UnityEditor;
using UnityEngine;

namespace TerraStreaming.MarkerComponents
{
	public class ImpostorObjectMarker : MonoBehaviour
	{
		
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ImpostorObjectMarker))]
	public class ImpostorObjectEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var impostorObject = (ImpostorObjectMarker)target;
			MeshRenderer[] meshRenderers = impostorObject.GetComponentsInChildren<MeshRenderer>();
			
			if (meshRenderers.Length == 0)
			{
				EditorGUILayout.HelpBox("This object doesn't have any MeshRenderers. Why did you add it?", MessageType.Info);
			}
		}
	}
#endif
}