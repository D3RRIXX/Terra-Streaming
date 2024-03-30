using System.Collections.Generic;
using UnityEngine;

namespace TerraStreaming
{
	[CreateAssetMenu(fileName = "New World Data", menuName = "Terra Streaming/World Data", order = 0)]
	public class WorldData : ScriptableObject
	{
		[SerializeField, SerializeReference] private List<ChunkData> _chunks;
		[SerializeField] private float _loadRange;
		[SerializeField] private Vector2Int _indexOffset;

		public float LoadRange => _loadRange;
	}
}