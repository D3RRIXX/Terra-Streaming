using System;
using System.Collections.Generic;
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
		private static bool[][] _chunkStates;

		static WorldGizmoDrawer()
		{
			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
			EditorApplication.playModeStateChanged += change => GetStreamingManager(SceneManager.GetActiveScene());
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

		private struct DrawChunkData
		{
			public int X, Y;
			public Vector3 CenterPos;
		}

		private static void DrawChunks(WorldData worldData, SceneView sceneView)
		{
			GridSettings gridSettings = worldData.GridSettings;

			if (_streamingManager.StreamingSources?.Count == 0)
			{
				using var drawingScope = new Handles.DrawingScope(Color.gray);
				foreach ((int x, int y) in gridSettings.EnumerateGrid())
				{
					var cellPosition = GridUtils.CellPosition(gridSettings, x, y);
					DrawChunk(sceneView, new DrawChunkData { CenterPos = cellPosition, X = x, Y = y });
				}

				return;
			}

			var cells =
				from coords in gridSettings.EnumerateGrid()
				let cellPosition = GridUtils.CellPosition(gridSettings, coords.x, coords.y)
				let drawData = new DrawChunkData { X = coords.x, Y = coords.y, CenterPos = cellPosition }
				group drawData by _streamingManager.StreamingSources.Select(x => GetState(cellPosition, x)).Max()
				into cellGroup
				orderby cellGroup.Key
				select cellGroup;

			foreach (IGrouping<ChunkState, DrawChunkData> grouping in cells.AsParallel())
			{
				using var drawingScope = new Handles.DrawingScope(GetGizmoColor(grouping.Key));

				foreach (DrawChunkData drawChunkData in grouping)
				{
					DrawChunk(sceneView, drawChunkData);
				}
			}

			return;

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

		private static void DrawChunk(SceneView sceneView, in DrawChunkData data)
		{
			var gridSettings = _streamingManager.WorldData.GridSettings;
			var size = new Vector3(gridSettings.CellSize, 0f, gridSettings.CellSize);
			Handles.DrawWireCube(data.CenterPos, size);

			if (!Application.isPlaying)
				DrawChunkToggle(sceneView, data);
		}

		private static void DrawChunkToggle(SceneView sceneView, in DrawChunkData data)
		{
			Handles.BeginGUI();

			Vector3 guiPos = sceneView.camera.WorldToScreenPoint(data.CenterPos);
			guiPos.y = sceneView.camera.pixelRect.height - guiPos.y;

			var rect = new Rect(guiPos.x, guiPos.y, 15f, 15f);
			bool toggleVal = EditorGUI.Toggle(rect, _chunkStates[data.X][data.Y]);

			if (toggleVal != _chunkStates[data.X][data.Y])
			{
				_chunkStates[data.X][data.Y] = toggleVal;
				_streamingManager.SetChunkEnabled(data.X, data.Y, value: toggleVal);
			}

			Handles.EndGUI();
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
			if (_streamingManager != null && _streamingManager.WorldData != null)
			{
				Vector2Int gridSize = _streamingManager.WorldData.GridSettings.GridSize;
				_chunkStates = new bool[gridSize.x][];
				for (int x = 0; x < _chunkStates.Length; x++)
				{
					_chunkStates[x] = new bool[gridSize.y];
				}
			}
		}
	}
}