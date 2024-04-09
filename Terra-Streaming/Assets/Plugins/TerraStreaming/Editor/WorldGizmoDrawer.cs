using System;
using System.Linq;
using TerraStreaming.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraStreaming
{
	[InitializeOnLoad]
	public static class WorldGizmoDrawer
	{
		private static StreamingManager _streamingManager;
		private static bool _testBool;

		static WorldGizmoDrawer()
		{
			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
			SceneView.duringSceneGui += OnSceneGUI;

			GetStreamingManager(SceneManager.GetActiveScene());
		}

		public static bool GizmosEnabled { get; set; }

		private static void OnSceneGUI(SceneView sceneView)
		{
			if (!GizmosEnabled || !_streamingManager)
				return;

			WorldData worldData = _streamingManager.WorldData;

			DrawStreamingSources(worldData);
			DrawChunks(worldData, sceneView);
		}

		private static void DrawChunks(WorldData worldData, SceneView sceneView)
		{
			GridSettings gridSettings = worldData.GridSettings;
			var size = new Vector3(gridSettings.CellSize, 0f, gridSettings.CellSize);

			if (_streamingManager.StreamingSources?.Count == 0)
			{
				using var drawingScope = new Handles.DrawingScope(Color.gray);
				foreach ((int x, int y) in gridSettings.EnumerateGrid())
				{
					var cellPosition = GridUtils.CellPosition(gridSettings, x, y);
					DrawChunk(cellPosition);
				}

				return;
			}

			var cells =
				from coords in gridSettings.EnumerateGrid()
				let cellPosition = GridUtils.CellPosition(gridSettings, coords.x, coords.y)
				group cellPosition by _streamingManager.StreamingSources.Select(x => GetState(cellPosition, x)).Max()
				into cellGroup
				orderby cellGroup.Key
				select cellGroup;

			foreach (IGrouping<ChunkState, Vector3> grouping in cells.AsParallel())
			{
				using var drawingScope = new Handles.DrawingScope(GetGizmoColor(grouping.Key));

				foreach (Vector3 cellPosition in grouping)
				{
					DrawChunk(cellPosition);
				}
			}

			return;

			void DrawChunk(Vector3 cellPosition)
			{
				Handles.DrawWireCube(cellPosition, size);

				DrawChunkToggle(cellPosition);
			}

			void DrawChunkToggle(Vector3 cellPosition)
			{
				Handles.BeginGUI();

				Vector3 guiPos = sceneView.camera.WorldToScreenPoint(cellPosition);
				guiPos.y = sceneView.camera.pixelRect.height - guiPos.y;
					
				var rect = new Rect(guiPos.x, guiPos.y, 10f, 10f);
				_testBool = EditorGUI.Toggle(rect, _testBool);

				Handles.EndGUI();
			}

			ChunkState GetState(Vector3 cellPos, StreamingSource streamingSource)
			{
				Vector3 sourcePosition = streamingSource.Position;
				sourcePosition.y = 0f;

				float closestX = Mathf.Clamp(sourcePosition.x, cellPos.x - gridSettings.CellSize / 2, cellPos.x + gridSettings.CellSize / 2);
				float closestZ = Mathf.Clamp(sourcePosition.z, cellPos.z - gridSettings.CellSize / 2, cellPos.z + gridSettings.CellSize / 2);

				var closestPos = new Vector3(closestX, 0f, closestZ);
				float distance = Vector3.Distance(sourcePosition, closestPos);

				if (distance <= worldData.LoadRange)
					return ChunkState.Regular;
				if (distance <= worldData.ImpostorLoadRange)
					return ChunkState.Impostor;

				return ChunkState.None;
			}
		}

		private static Color GetGizmoColor(ChunkState chunkState)
		{
			return chunkState switch
			{
				ChunkState.None => Color.gray,
				ChunkState.Impostor => Color.yellow,
				ChunkState.Regular => Color.green,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		private static void DrawStreamingSources(WorldData worldData)
		{
			foreach (StreamingSource streamingSource in _streamingManager.StreamingSources)
			{
				DrawArc(streamingSource, worldData.LoadRange, GetGizmoColor(ChunkState.Regular));
				DrawArc(streamingSource, worldData.ImpostorLoadRange, GetGizmoColor(ChunkState.Impostor));
			}

			return;

			void DrawArc(StreamingSource streamingSource, float radius, Color color)
			{
				Handles.color = color;
				Handles.DrawWireArc(streamingSource.Position, Vector3.up, Vector3.forward, 360f, radius);
			}
		}

		private static void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
		{
			GetStreamingManager(currentScene);
		}

		private static void GetStreamingManager(Scene currentScene)
		{
			_streamingManager = StreamingManager.Instance;
		}
	}
}