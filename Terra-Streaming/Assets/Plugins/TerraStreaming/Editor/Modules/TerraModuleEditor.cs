using UnityEngine;

namespace TerraStreaming.Modules
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