using System;
using UnityEngine;

namespace TerraStreaming
{
	[Serializable]
	public class ChunkData
	{
		[SerializeField, SerializeReference] private ChunkData _leftNeighbour;
		[SerializeField, SerializeReference] private ChunkData _rightNeighbour;
		[SerializeField, SerializeReference] private ChunkData _topNeighbour;
		[SerializeField, SerializeReference] private ChunkData _bottomNeighbour;

		public string SceneName;
		public Vector2Int Coords;

		public ChunkData LeftNeighbour
		{
			get => _leftNeighbour;
			set
			{
				_leftNeighbour = value;
				if (value != null)
					value._rightNeighbour = this;
			}
		}

		public ChunkData RightNeighbour
		{
			get => _rightNeighbour;
			set
			{
				_rightNeighbour = value;
				if (value != null)
					value._leftNeighbour = this;
			}
		}

		public ChunkData TopNeighbour
		{
			get => _topNeighbour;
			set
			{
				_topNeighbour = value;
				if (value != null)
					value._bottomNeighbour = this;
			}
		}
		
		public ChunkData BottomNeighbour
		{
			get => _bottomNeighbour;
			set
			{
				_bottomNeighbour = value;
				if (value != null)
					value._topNeighbour = this;
			}
		}
	}
}