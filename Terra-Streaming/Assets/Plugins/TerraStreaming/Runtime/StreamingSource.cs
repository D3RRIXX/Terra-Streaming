using UnityEngine;

namespace TerraStreaming
{
	[ExecuteAlways]
	public class StreamingSource : MonoBehaviour
	{
		[SerializeField] private Vector3 _centerOffset;
		[SerializeField] private bool _autoRegister = true;
		
		private bool _registered;

		public Vector3 Position => transform.position + _centerOffset;

		private void OnEnable()
		{
			if (_autoRegister)
				RegisterSelf();
		}

		private void OnDisable()
		{
			UnregisterSelf();
		}

		public void RegisterSelf()
		{
			if (_registered)
				return;
			
			StreamingManager.Instance.RegisterStreamingSource(this);
			_registered = true;
		}

		public void UnregisterSelf()
		{
			if (!_registered)
				return;
			
			StreamingManager.Instance.UnregisterStreamingSource(this);
			_registered = false;
		}
	}
}