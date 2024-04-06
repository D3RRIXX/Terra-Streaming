using UnityEngine;

namespace TerraStreamer._Terrain_Tests_.Scripts.TerraStreamer.Editor.Modules
{
	public abstract class TerraModuleEditor<T> : UnityEditor.Editor where T : ScriptableObject
	{
		protected T Module { get; private set; }

		private void OnEnable()
		{
			if (target == null)
				return;
			
			Module = (T)target;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			DrawPropertiesExcluding(serializedObject, "m_Script");

			serializedObject.ApplyModifiedProperties();
		}
	}
}