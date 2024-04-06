using System.Collections.Generic;
using TerraStreaming.Editor;
using UnityEngine;

namespace TerraStreamer.Data
{
	[CreateAssetMenu(fileName = "New World Data", menuName = "Terra/World Data")]
	public class WorldData : ScriptableObject
	{
		[SerializeField, SerializeReference] private List<ChunkData> _chunkData = new();
		[SerializeField] private float _loadRange;
		public GridSettings GridSettings;

		public float LoadRange => _loadRange;
		public IReadOnlyList<ChunkData> ChunkDataList => _chunkData;
		public string WorldName => name.Split('_')[0];

#if UNITY_EDITOR
		public void AddChunkData(ChunkData chunkData)
		{
			_chunkData.Add(chunkData);
		}

		public void UpdateChunkBounds()
		{
			foreach (ChunkData chunkData in _chunkData)
			{
				chunkData.SetupBounds();
			}
		}
#endif
	}
}