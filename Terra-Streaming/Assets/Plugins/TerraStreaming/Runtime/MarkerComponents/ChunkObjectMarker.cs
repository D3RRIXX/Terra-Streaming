using UnityEngine;

namespace TerraStreaming.MarkerComponents
{
	public enum ObjectCollectionType
	{
		SingleObject,
		CollectionParent
	}

	public class ChunkObjectMarker : MonoBehaviour
	{
		public ObjectCollectionType ObjectCollectionType;

		public static ChunkObjectMarker AddTo(GameObject gameObject, ObjectCollectionType collectionType)
		{
			var marker = gameObject.AddComponent<ChunkObjectMarker>();
			marker.ObjectCollectionType = collectionType;
			return marker;
		}
	}
}