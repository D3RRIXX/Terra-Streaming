using System.Linq;
using UnityEngine;

namespace TerraStreaming
{
	public class StreamingManager : MonoBehaviour
	{
		[SerializeField] private StreamingSource[] _streamingSources;
		[SerializeField] private GameObject[] _streamableObjects;
		[SerializeField] private float _loadDistance;

		private void Start()
		{
			_streamingSources = FindObjectsOfType<StreamingSource>();
		}

		private void OnDrawGizmos()
		{
			foreach (StreamingSource streamingSource in _streamingSources)
			{
				Gizmos.DrawWireSphere(streamingSource.Position, _loadDistance);
			}

			foreach (GameObject obj in _streamableObjects)
			{
				Gizmos.color = obj.activeSelf ? Color.green : Color.red;
				Gizmos.DrawWireCube(obj.transform.position, Vector3.one);
			}
		}

		private void Update()
		{
			foreach (GameObject streamableObject in _streamableObjects)
			{
				bool shouldEnable = ShouldEnable(streamableObject.transform.position);
				if (shouldEnable == streamableObject.activeSelf)
					continue;

				streamableObject.SetActive(shouldEnable);
			}
		}

		private bool ShouldEnable(Vector3 position)
		{
			return _streamingSources.Select(x => Vector3.Distance(x.Position, position))
			                        .Any(x => x <= _loadDistance);
		}
	}
}