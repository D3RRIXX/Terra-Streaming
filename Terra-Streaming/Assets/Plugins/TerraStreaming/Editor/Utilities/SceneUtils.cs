using System;
using System.Linq;
using System.Text;
using TerraStreamer.Editor.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Editor
{
	public static class SceneUtils
	{
		public static string GetParentFolder(string path)
		{
			var targetPath = path.Split('/').SkipLast(1);
			using var builderScope = new StringBuilderScope();
			builderScope.Builder.AppendJoin('/', targetPath);

			return builderScope.Builder.ToString();
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