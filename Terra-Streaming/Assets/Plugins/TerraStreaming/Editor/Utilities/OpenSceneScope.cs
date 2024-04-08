using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TerraStreaming.Utilities
{
	public class OpenSceneScope : IDisposable
	{
		private readonly bool _closeOnDispose;
		private readonly string _dtsPath;

		public static OpenSceneScope Existing(string scenePath, bool closeOnDispose, OpenSceneMode openSceneMode = OpenSceneMode.Additive)
		{
			Scene scene = EditorSceneManager.OpenScene(scenePath, openSceneMode);
			return new OpenSceneScope(scene, closeOnDispose, scenePath);
		}

		public static OpenSceneScope New(string saveScenePath, bool closeOnDispose, NewSceneMode newSceneMode = NewSceneMode.Additive)
		{
			Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, newSceneMode);
			return new OpenSceneScope(scene, closeOnDispose, saveScenePath);
		}

		private OpenSceneScope(in Scene scene, bool closeOnDispose, string dtsPath)
		{
			_closeOnDispose = closeOnDispose;
			_dtsPath = dtsPath;
			Scene = scene;
		}

		public Scene Scene { get; }

		public void Dispose()
		{
			EditorSceneManager.SaveScene(Scene, _dtsPath);

			if (_closeOnDispose)
				EditorSceneManager.CloseScene(Scene, true);
		}
	}
}