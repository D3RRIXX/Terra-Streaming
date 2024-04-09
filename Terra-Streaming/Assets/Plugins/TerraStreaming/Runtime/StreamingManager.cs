using System.Collections.Generic;
using System.Linq;
using TerraStreaming.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerraStreaming
{
	public partial class StreamingManager : MonoBehaviour
	{
		[SerializeField] private WorldData _worldData;

		[SerializeField] private List<StreamingSource> _streamingSources = new();
		[Header("Load Distances")]
		[SerializeField] private float _regularLoadDistance = 100f;
		[SerializeField] private float _impostorLoadDistance = 100f;

		private ChunkLoader _chunkLoader;
		private NativeArray<Bounds> _bounds;
		private NativeArray<ChunkState> _resultArray;
		private JobHandle _handle;

		private static StreamingManager instance;

		public static StreamingManager Instance
		{
			get
			{
#if UNITY_EDITOR
				if (instance == null)
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

			_bounds = new NativeArray<Bounds>(chunkDataList.Select(x => x.Bounds).ToArray(), Allocator.Persistent);
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
			if (!Application.isPlaying)
				return;

			UpdateWithJobs();
		}

		private void LateUpdate()
		{
			if (!Application.isPlaying)
				return;

			_handle.Complete();

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

			var handle = (JobHandle)default;
			foreach (StreamingSource streamingSource in _streamingSources)
			{
				var job = new GetChunkLoadStateJob(streamingSource.Position, _bounds, _impostorLoadDistance, _regularLoadDistance, _resultArray);
				handle = job.Schedule(length, 1, handle);
			}

			_handle = handle;
		}

	}
}