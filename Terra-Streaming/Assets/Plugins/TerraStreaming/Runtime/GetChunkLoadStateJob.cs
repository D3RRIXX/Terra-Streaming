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
		[ReadOnly] private readonly NativeArray<float3> _streamingSourcePositions;
		[ReadOnly] private NativeArray<Bounds> _boundsArray;
		[ReadOnly] private readonly float _impostorLoadRange;
		[ReadOnly] private readonly float _regularLoadRange;

		private NativeArray<ChunkState> _outputArray;

		public GetChunkLoadStateJob(NativeArray<float3> streamingSourcePositions, NativeArray<Bounds> boundsArray, float impostorLoadRange,
			float regularLoadRange, NativeArray<ChunkState> outputArray)
		{
			_streamingSourcePositions = streamingSourcePositions;
			_boundsArray = boundsArray;
			_impostorLoadRange = impostorLoadRange;
			_regularLoadRange = regularLoadRange;
			_outputArray = outputArray;
		}

		public void Execute(int index)
		{
			Bounds bounds = _boundsArray[index];

			var highestState = ChunkState.None;
			foreach (float3 playerPos in _streamingSourcePositions)
			{
				float3 closestPoint = bounds.ClosestPoint(playerPos);

				float distance = math.distance(closestPoint, playerPos);

				if (distance <= _regularLoadRange)
					highestState = GetHighestState(ChunkState.Regular);
				else if (distance <= _impostorLoadRange)
					highestState = GetHighestState(ChunkState.Impostor);
			}

			_outputArray[index] = highestState;
			return;

			ChunkState GetHighestState(ChunkState newState) => highestState < newState ? newState : highestState;
		}
	}
}