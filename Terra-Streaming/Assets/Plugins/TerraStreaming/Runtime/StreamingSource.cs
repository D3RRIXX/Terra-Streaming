using UnityEngine;

namespace TerraStreaming
{
    public class StreamingSource : MonoBehaviour
    {
        [SerializeField] private Vector3 _centerOffset;

        public Vector3 Position => transform.position + _centerOffset;
    }
}
