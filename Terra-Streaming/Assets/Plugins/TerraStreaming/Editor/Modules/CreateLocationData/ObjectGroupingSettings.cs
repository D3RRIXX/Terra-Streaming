using System.Collections.Generic;
using UnityEngine;

namespace TerraStreaming.Modules.CreateLocationData
{
	[System.Serializable]
	public class ObjectGroupingSettings
	{
		public List<Transform> Parents = new();
		public List<Transform> IndividualObjects = new();
	}
}