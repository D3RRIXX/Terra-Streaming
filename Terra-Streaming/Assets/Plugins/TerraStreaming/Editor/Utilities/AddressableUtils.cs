using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace TerraStreamer.Editor.Utilities
{
	public static class AddressableUtils
	{
		/// <summary>
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="path"></param>
		/// <returns>GUID to that asset</returns>
		public static string AddAssetToAddressables(string groupName, string path)
		{
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
				return string.Empty;

			AddressableAssetGroup group = GetOrCreateGroup(settings, groupName);
			string guid = AssetDatabase.GUIDFromAssetPath(path).ToString();

			MoveGUIDToGroup(settings, guid, group);
			return guid;
		}

		public static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName) 
			=> settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema));

		public static void MoveGUIDToGroup(AddressableAssetSettings settings, string guid, AddressableAssetGroup group)
		{
			AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
			var entriesAdded = new List<AddressableAssetEntry> { entry };

			group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false);
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false);
		}
	}
}