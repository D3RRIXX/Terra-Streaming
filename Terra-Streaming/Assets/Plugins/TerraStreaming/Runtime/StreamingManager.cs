using System.Collections.Generic;
using System.Linq;
using TerraStreaming.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TerraStreaming
{
	public partial class StreamingManager : MonoBehaviour
	{
		[SerializeField] private WorldData _worldData;

		[SerializeField] private List<StreamingSource> _streamingSources = new();

		private ChunkLoader _chunkLoader;
		
		private NativeArray<Bounds> _bounds;
		private NativeArray<ChunkState> _resultArray;
		private NativeArray<float3> _streamingSourcesPositions;
		
		private JobHandle _handle;

		private static StreamingManager instance;

		public static StreamingManager Instance
		{
			get
			{
#if UNITY_EDITOR
				if (instance == null && !Application.isPlaying)
					instance = FindObjectOfType<StreamingManager>();
#endif
				return instance;
			}
			private set => instance = value;
		}

		public WorldData WorldData => _worldData;
		public IReadOnlyList<StreamingSource> StreamingSources => _streamingSources;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else
				DestroyImmediate(gameObject);

			var chunkDataList = _worldData.ChunkDataList;

			_bounds = new NativeArray<Bounds>(chunkDataList.Select(x => x.Coords).Select(_worldData.GetChunkBounds).ToArray(), Allocator.Persistent);
			_resultArray = new NativeArray<ChunkState>(_bounds.Length, Allocator.Persistent);

			_chunkLoader = new ChunkLoader();
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
			_streamingSourcesPositions.Dispose();

			for (int i = 0; i < _resultArray.Length; i++)
			{
				ChunkData chunkData = _worldData.ChunkDataList[i];
				_chunkLoader.UpdateChunk(chunkData, _resultArray[i]);
			}
		}

		public void RegisterStreamingSource(StreamingSource streamingSource)
		{
			if (_streamingSources.Contains(streamingSource))
				return;

			_streamingSources.Add(streamingSource);
		}

		public void UnregisterStreamingSource(StreamingSource streamingSource)
		{
			_streamingSources.Remove(streamingSource);
		}

		private void UpdateWithJobs()
		{
			int length = _bounds.Length;

			_streamingSourcesPositions = new NativeArray<float3>(_streamingSources.Select(x => (float3)x.Position).ToArray(), Allocator.TempJob);
			var job = new GetChunkLoadStateJob(_streamingSourcesPositions, _bounds, _worldData.ImpostorLoadRange, _worldData.LoadRange, _resultArray);
			_handle = job.Schedule(length, 1);
		}
	}
}