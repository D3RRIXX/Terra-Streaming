using System.Collections.Generic;
using UnityEngine;

namespace TerraStreaming.Data
{
	[CreateAssetMenu(fileName = "New World Data", menuName = "Terra/World Data")]
	public class WorldData : ScriptableObject
	{
		[SerializeField, Min(0)] private float _loadRange;
		[SerializeField, Min(0)] private float _impostorLoadRange;
		
		[SerializeField, SerializeReference] private List<ChunkData> _chunkData = new();
		public GridSettings GridSettings;

		public float LoadRange => _loadRange;
		public float ImpostorLoadRange => _impostorLoadRange;
		
		public IReadOnlyList<ChunkData> ChunkDataList => _chunkData;

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