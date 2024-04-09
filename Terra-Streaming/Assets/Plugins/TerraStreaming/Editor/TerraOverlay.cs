using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace TerraStreaming
{
	[Overlay(typeof(SceneView), "terra-overlay", "Terra Overlay")]
	public class TerraOverlay : Overlay
	{
		public override VisualElement CreatePanelContent()
		{
			var root = new VisualElement { name = "content" };
			var enableGizmosToggle = new Toggle("Enable Gizmos")
			{
				name = "enable-gizmos__toggle",
				viewDataKey = "terra-enable-gizmos"
			};
			enableGizmosToggle.RegisterValueChangedCallback(evt =>
			{
				WorldGizmoDrawer.GizmosEnabled = evt.newValue;
			});
			
			root.Add(enableGizmosToggle);
			
			return root;
		}
	}

	[EditorToolbarElement("ExampleToolbar/Button", typeof(SceneView))]
	public class MyToolbarOverlay : EditorToolbarButton
	{
		
	}
}