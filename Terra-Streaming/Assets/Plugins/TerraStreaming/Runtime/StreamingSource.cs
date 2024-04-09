using UnityEngine;

namespace TerraStreaming
{
	[ExecuteAlways]
	public class StreamingSource : MonoBehaviour
	{
		[SerializeField] private Vector3 _centerOffset;

		public Vector3 Position => transform.position + _centerOffset;

		private void OnEnable()
		{
			WorldGizmoDrawer.RegisterStreamingSource(this);
		}

		private void OnDestroy()
		{
			WorldGizmoDrawer.UnregisterStreamingSource(this);
		}
	}
}