using System.Collections.Generic;
using UnityEngine;

namespace TerraStreaming.Editor
{
	[System.Serializable]
	public class ObjectGroupingSettings
	{
		public List<Transform> Parents = new();
		public List<Transform> IndividualObjects = new();
	}
}