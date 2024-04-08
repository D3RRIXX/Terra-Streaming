using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TerraStreaming.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace TerraStreaming
{
	public class ChunkLoader
	{
		private readonly Dictionary<ChunkData, AsyncOperationHandle<SceneInstance>> _loadedChunksMap = new();
		private readonly Dictionary<ChunkData, AsyncOperationHandle<GameObject>> _loadedImpostorsMap = new();

		private readonly HashSet<ChunkData> _chunksInLoadProgress = new();

		public void UpdateChunk(ChunkData chunkData, ChunkState chunkState)
		{
			if (_chunksInLoadProgress.Contains(chunkData))
				return;

			switch (chunkState)
			{
				case ChunkState.None:
					TryUnloadImpostor(chunkData);
					break;
				case ChunkState.Regular:
					TryLoadChunkAsync(chunkData);
					break;
				case ChunkState.Impostor:
					TryLoadImpostorAsync(chunkData);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private async void TryUnloadChunkAsync(ChunkData chunkData)
		{
			if (!_loadedChunksMap.ContainsKey(chunkData))
				return;

			_chunksInLoadProgress.Add(chunkData);
			AsyncOperationHandle<SceneInstance> handle = _loadedChunksMap[chunkData];

			await Addressables.UnloadSceneAsync(handle.Result, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects).ToUniTask();
			await Resources.UnloadUnusedAssets();

			Debug.Log($"Unloaded chunk '{chunkData.Name}'");

			_chunksInLoadProgress.Remove(chunkData);
			_loadedChunksMap.Remove(chunkData);
		}

		private async void TryLoadImpostorAsync(ChunkData chunkData)
		{
			if (string.IsNullOrWhiteSpace(chunkData.ImpostorPrefab.AssetGUID))
				return;

			if (_loadedImpostorsMap.ContainsKey(chunkData))
				return;

			_chunksInLoadProgress.Add(chunkData);
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(chunkData.ImpostorPrefab);

			await handle.ToUniTask();
			// Debug.Log($"Loaded impostor '{chunkData.ImpostorPrefab.Asset.name}'");

			_chunksInLoadProgress.Remove(chunkData);
			_loadedImpostorsMap.Add(chunkData, handle);

			if (_loadedChunksMap.ContainsKey(chunkData))
				TryUnloadChunkAsync(chunkData);
		}

		private async void TryLoadChunkAsync(ChunkData chunkData)
		{
			if (_loadedChunksMap.ContainsKey(chunkData))
				return;

			_chunksInLoadProgress.Add(chunkData);
			var handle = Addressables.LoadSceneAsync(chunkData.SceneRef, LoadSceneMode.Additive);

			await handle.ToUniTask();
			// Debug.Log($"Loaded chunk '{chunkData.name}'");

			TryUnloadImpostor(chunkData);

			_chunksInLoadProgress.Remove(chunkData);
			_loadedChunksMap.Add(chunkData, handle);
		}

		private void TryUnloadImpostor(ChunkData chunkData)
		{
			if (!_loadedImpostorsMap.ContainsKey(chunkData))
				return;
			
			Addressables.Release(_loadedImpostorsMap[chunkData]);
			_loadedImpostorsMap.Remove(chunkData);
		}
	}
}
