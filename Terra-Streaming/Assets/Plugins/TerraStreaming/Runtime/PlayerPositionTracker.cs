using System.Linq;
using ChunkLoadSystem;
using TerraStreamer.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerraStreamer
{
	public class PlayerPositionTracker : MonoBehaviour
	{
		[SerializeField] private WorldData _worldData;
		[SerializeField] private ChunkLoader _chunkLoader;

		[SerializeField] private Transform _target;
		[Header("Load Distances")] 
		[SerializeField] private float _regularLoadDistance = 100f;
		[SerializeField] private float _impostorLoadDistance = 100f;

		private NativeArray<Bounds> _bounds;
		private NativeArray<ChunkState> _resultArray;
		private JobHandle _handle;

		private void Awake()
		{
			var chunkDataList = _worldData.ChunkDataList;

			_bounds = new NativeArray<Bounds>(chunkDataList.Select(x => x.Bounds).ToArray(), Allocator.Persistent);
			_resultArray = new NativeArray<ChunkState>(_bounds.Length, Allocator.Persistent);
		}
		
		private void OnDestroy()
		{
			_bounds.Dispose();
			_resultArray.Dispose();
		}

		private void Update()
		{
			UpdateWithJobs();
		}

		private void LateUpdate()
		{
			_handle.Complete();

			for (int i = 0; i < _resultArray.Length; i++)
			{
				ChunkData chunkData = _worldData.ChunkDataList[i];
				_chunkLoader.UpdateChunk(chunkData, _resultArray[i]);
			}
		}
		
		private void UpdateWithJobs()
		{
			int length = _bounds.Length;

			var job = new GetChunkLoadStateJob(_target.position, _bounds, _impostorLoadDistance, _regularLoadDistance,
				_resultArray);

			_handle = job.Schedule(length, 1);
		}
	}
}