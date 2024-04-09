using System.Linq;
using TerraStreaming.Data;
using UnityEngine;

namespace TerraStreaming.Utilities
{
	public static class Utils
	{
		public static ChunkData GetChunkAt(this WorldData worldData, int x, int y) => worldData.ChunkDataList.First(chunk => chunk.Coords == new Vector2Int(x, y));
	}
}