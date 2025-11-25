using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace K10.EditorUtils
{
    public static class AddressablesLib
    {
        public static void AddToGroup(string assetPath, string groupName = "Default Local Group")
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found. Did you create Addressables in the project?");
                return;
            }

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(BundledAssetGroupSchema));
            }

            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            entry.SetAddress(assetPath);

            EditorUtility.SetDirty(settings);
        }
    }
}