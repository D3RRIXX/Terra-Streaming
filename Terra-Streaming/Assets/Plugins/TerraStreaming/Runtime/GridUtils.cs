using System.Collections.Generic;
using TerraStreaming.Data;
using UnityEngine;

namespace TerraStreaming
{
	public static class GridUtils
	{
		public static Vector3 CellPosition(GridSettings gridSettings, int x, int y)
		{
			Vector3 centerOffset = CalculateCenter(gridSettings);
			Vector3 pos = new(x * gridSettings.CellSize, 0f, y * gridSettings.CellSize);

			return pos + centerOffset + gridSettings.CenterOffset;
		}

		public static IEnumerable<(int x, int y)> EnumerateGrid(this GridSettings gridSettings)
		{
			for (int x = 0; x < gridSettings.GridSize.x; x++)
			for (int y = 0; y < gridSettings.GridSize.y; y++)
			{
				yield return (x, y);
			}
		}

		private static Vector3 CalculateCenter(GridSettings gridSettings)
		{
			Vector3 centerOffset = Vector3.one * (gridSettings.CellSize / 2f);
			centerOffset.y = 0f;
			return centerOffset;
		}
	}
}