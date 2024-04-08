using UnityEngine;

namespace TerraStreaming.Data
{
	[System.Serializable]
	public struct GridSettings
	{
		public float CellSize;
		public Vector2Int GridSize;
		public Vector3 CenterOffset;
	}
}