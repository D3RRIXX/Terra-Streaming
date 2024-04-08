using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TerraStreaming
{
	[BurstCompile]
	public struct GetChunkLoadStateJob : IJobParallelFor
	{
		[ReadOnly] private readonly Vector3 _playerPos;
		[ReadOnly] private NativeArray<Bounds> _boundsArray;
		[ReadOnly] private readonly float _impostorLoadDistance;
		[ReadOnly] private readonly float _regularLoadDistance;
	
		private NativeArray<ChunkState> _outputArray;

		public GetChunkLoadStateJob(Vector3 playerPos, NativeArray<Bounds> boundsArray, float impostorLoadDistance,
			float regularLoadDistance, NativeArray<ChunkState> outputArray)
		{
			_playerPos = playerPos;
			_boundsArray = boundsArray;
			_impostorLoadDistance = impostorLoadDistance;
			_regularLoadDistance = regularLoadDistance;
			_outputArray = outputArray;
		}

		public void Execute(int index)
		{
			Bounds bounds = _boundsArray[index];
			float3 closestPoint = bounds.ClosestPoint(_playerPos);
			
			float distance = math.distance(closestPoint, _playerPos);

			if (distance <= _regularLoadDistance)
				AssignIfGreater(index, ChunkState.Regular);
			else if (distance <= _impostorLoadDistance)
				AssignIfGreater(index, ChunkState.Impostor);
			else
				AssignIfGreater(index, ChunkState.None);
		}

		private void AssignIfGreater(int index, ChunkState newState)
		{
			if (newState > _outputArray[index])
				_outputArray[index] = newState;
		}
	}
}