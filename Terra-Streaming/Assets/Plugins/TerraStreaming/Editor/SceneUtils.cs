using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Editor
{
	public static class SceneUtils
	{
		public static string GetOrCreateFolder(string parentFolder, string folderName)
		{
			string combinedPath = $"{parentFolder}/{folderName}";
			if (AssetDatabase.IsValidFolder(combinedPath))
				return combinedPath;

			string guid = AssetDatabase.CreateFolder(parentFolder, folderName);
			return string.IsNullOrEmpty(guid) ? string.Empty : AssetDatabase.GUIDToAssetPath(guid);
		}

		public static string GetParentFolder(this Scene scene)
		{
			var targetPath = scene.path.Split('/').SkipLast(1);
			var builder = new StringBuilder();
			builder.AppendJoin('/', targetPath);

			return builder.ToString();
		}

		public static bool AssetExists(string path) => AssetDatabase.LoadMainAssetAtPath(path) != null;

		public static OpenSceneScope GetOrCreateScene(string scenePath, OpenSceneMode openSceneMode = OpenSceneMode.Additive)
		{
			if (AssetExists(scenePath))
				return OpenSceneScope.Existing(scenePath, openSceneMode);

			var newSceneMode = openSceneMode switch
			{
				OpenSceneMode.Single => NewSceneMode.Single,
				OpenSceneMode.Additive or OpenSceneMode.AdditiveWithoutLoading => NewSceneMode.Additive,
				_ => throw new ArgumentOutOfRangeException(nameof(openSceneMode), openSceneMode, null)
			};
			
			return OpenSceneScope.New(newSceneMode);
		}
	}

	public class OpenSceneScope : IDisposable
	{
		public static OpenSceneScope Existing(string scenePath, OpenSceneMode openSceneMode = OpenSceneMode.Additive) 
			=> new(EditorSceneManager.OpenScene(scenePath, openSceneMode));

		public static OpenSceneScope New(NewSceneMode newSceneMode = NewSceneMode.Additive) 
			=> new(EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, newSceneMode));

		private OpenSceneScope(in Scene scene)
		{
			Scene = scene;
		}

		public Scene Scene { get; }

		public void Dispose()
		{
			EditorSceneManager.SaveScene(Scene);
			EditorSceneManager.CloseScene(Scene, false);
		}
	}
}