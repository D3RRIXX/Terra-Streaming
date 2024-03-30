using UnityEngine;

namespace TerraStreaming.Editor
{
	[System.Serializable]
	public struct ObjectGroupingSettings
	{
		public Transform[] Parents;
		public Transform[] IndividualObjects;
	}
}