using System;
using System.Linq;
using TerraStreaming.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Utilities
{
	public static class SceneUtils
	{
		public static WorldData GetAssociatedWorldData(this Scene scene)
		{
			string parentFolder = Utils.GetParentFolder(scene.path);
			return AssetDatabase.LoadAllAssetsAtPath(parentFolder).OfType<WorldData>().SingleOrDefault();
		}

		public static OpenSceneScope GetOrCreateScene(string scenePath, bool closeOnDispose, OpenSceneMode openSceneMode = OpenSceneMode.Additive)
		{
			if (Utils.AssetExists(scenePath, out _))
				return OpenSceneScope.Existing(scenePath, closeOnDispose, openSceneMode);

			var newSceneMode = openSceneMode switch
			{
				OpenSceneMode.Single => NewSceneMode.Single,
				OpenSceneMode.Additive or OpenSceneMode.AdditiveWithoutLoading => NewSceneMode.Additive,
				_ => throw new ArgumentOutOfRangeException(nameof(openSceneMode), openSceneMode, null)
			};
			
			return OpenSceneScope.New(scenePath, closeOnDispose, newSceneMode);
		}
	}
}