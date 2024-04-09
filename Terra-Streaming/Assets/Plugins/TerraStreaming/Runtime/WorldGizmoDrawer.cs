using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using System.Collections.Generic;
using TerraStreaming.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
#endif

namespace TerraStreaming
{
	[InitializeOnLoad]
	public static class WorldGizmoDrawer
	{
#if UNITY_EDITOR
		private static readonly List<StreamingSource> STREAMING_SOURCES = new();
		private static StreamingManager _streamingManager;

		static WorldGizmoDrawer()
		{
			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
			SceneView.duringSceneGui += OnSceneGUI;

			GetStreamingManager(SceneManager.GetActiveScene());
		}

		private static void OnSceneGUI(SceneView obj)
		{
			if (!_streamingManager)
				return;

			WorldData worldData = _streamingManager.WorldData;

			DrawStreamingSources(worldData);
			DrawChunks(worldData);
		}

		private static void DrawChunks(WorldData worldData)
		{
			GridSettings gridSettings = worldData.GridSettings;
			var size = new Vector3(gridSettings.CellSize, 0f, gridSettings.CellSize);

			var cells =
				from coords in gridSettings.EnumerateGrid()
				let cellPosition = GridUtils.CellPosition(gridSettings, coords.x, coords.y)
				group cellPosition by GetState(cellPosition, STREAMING_SOURCES[0])
				into cellGroup
				orderby cellGroup.Key
				select cellGroup;

			foreach (IGrouping<ChunkState, Vector3> grouping in cells)
			{
				Handles.color = GetGizmoColor(grouping.Key);

				foreach (Vector3 cellPosition in grouping)
				{
					Handles.DrawWireCube(cellPosition, size);
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

			Color GetGizmoColor(ChunkState chunkState)
			{
				return chunkState switch
				{
					ChunkState.None => Color.grey,
					ChunkState.Impostor => Color.yellow,
					ChunkState.Regular => Color.green,
					_ => throw new ArgumentOutOfRangeException()
				};
			}
		}

		private static void DrawStreamingSources(WorldData worldData)
		{
			foreach (StreamingSource streamingSource in STREAMING_SOURCES)
			{
				DrawArc(streamingSource, worldData.LoadRange, Color.white);
				DrawArc(streamingSource, worldData.ImpostorLoadRange, Color.yellow);
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
			StreamingManager streamingManager = currentScene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StreamingManager>()).FirstOrDefault();
			_streamingManager = streamingManager;
		}
#endif

		[Conditional("UNITY_EDITOR")]
		public static void RegisterStreamingSource(StreamingSource streamingSource)
		{
			STREAMING_SOURCES.Add(streamingSource);
		}

		[Conditional("UNITY_EDITOR")]
		public static void UnregisterStreamingSource(StreamingSource streamingSource)
		{
			STREAMING_SOURCES.Remove(streamingSource);
		}
	}
}