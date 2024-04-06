using System.Collections.Generic;
using UnityEngine;

namespace TerraStreaming.Editor
{
	[System.Serializable]
	public struct ObjectGroupingSettings
	{
		public List<Transform> Parents;
		public List<Transform> IndividualObjects;
	}
}