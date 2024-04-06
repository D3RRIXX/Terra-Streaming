using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TerraStreamer.Data;

namespace TerraStreaming
{
	public static class ChunkDataExtensions
	{
		public static IEnumerable<ChunkData> IterateLeft([NotNull] this ChunkData chunkData) => chunkData.Iterate(data => data.LeftNeighbour);
		public static IEnumerable<ChunkData> IterateRight([NotNull] this ChunkData chunkData) => chunkData.Iterate(data => data.RightNeighbour);
		public static IEnumerable<ChunkData> IterateTop([NotNull] this ChunkData chunkData) => chunkData.Iterate(data => data.TopNeighbour);
		public static IEnumerable<ChunkData> IterateBottom([NotNull] this ChunkData chunkData) => chunkData.Iterate(data => data.BottomNeighbour);

		private static IEnumerable<ChunkData> Iterate([NotNull] this ChunkData chunkData, Func<ChunkData, ChunkData> selector)
		{
			ChunkData current = chunkData;
			while (current != null)
			{
				yield return current;
				current = selector(current);
			}
		}
	}
}