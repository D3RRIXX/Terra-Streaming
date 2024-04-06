using System;
using System.Collections.Generic;
using System.Linq;
using TerraStreamer;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TerraStreaming.Editor
{
	public static class ImpostorExtensions
	{
		private static readonly Type[] NON_REMOVABLE_TYPES =
		{
			typeof(MeshRenderer), typeof(MeshFilter),
			typeof(GameObject), typeof(Transform)
		};
		private static readonly HashSet<Type> NON_REMOVABLE_TYPES_SET = new(NON_REMOVABLE_TYPES);

		public static GameObject InstantiateStrippedImpostor(this ImpostorObjectMarker impostorObject)
		{
			var lodGroup = impostorObject.GetComponentInChildren<LODGroup>();
			GameObject sourceObj;
			if (lodGroup != null)
			{
				sourceObj = GetLowestLOD(lodGroup);
			}
			else
			{
				sourceObj = impostorObject.GetComponentInChildren<MeshRenderer>().gameObject;
			}

			Transform impostorTransform = impostorObject.transform;
			GameObject clone = Object.Instantiate(sourceObj, impostorTransform.position, impostorTransform.rotation);

			StripClone(clone);

			return clone;
		}

		private static GameObject GetLowestLOD(LODGroup lodGroup)
		{
			LOD lowestLod = lodGroup.GetLODs()[lodGroup.lodCount - 1];
			return lowestLod.renderers.First().gameObject;
		}

		private static void StripClone(GameObject gameObject)
		{
			Component[] components = gameObject.GetComponents<Component>();
			foreach (var component in components.Where(x => !NON_REMOVABLE_TYPES_SET.Contains(x.GetType())))
			{
				Object.DestroyImmediate(component);
			}

			var meshRenderer = gameObject.GetComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
	}
}