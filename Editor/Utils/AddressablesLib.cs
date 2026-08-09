using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace K10.EditorUtils
{
    public static class AddressablesLib
    {
        public static bool IsAssetAddressable(this Object obj)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
            return entry != null;
        }

        public static bool TryGetAddressableEntry(this Object obj, out AddressableAssetEntry entry)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
            return entry != null;
        }

        public static void SetAddressableGroupAndLabel(this Object obj, string groupName, string labelName = null, bool removeOtherLabels = false)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            var entry = settings.FindAssetEntry(guid);

            var needsGroupChange = entry == null || entry.parentGroup?.Name != groupName;
            var needsLabelChange = !string.IsNullOrEmpty(labelName) &&
                                   (entry == null || (!entry.labels.Contains(labelName) || removeOtherLabels && entry.labels.Count > 1));

            if (!needsGroupChange && !needsLabelChange) return;

            Undo.RecordObject(settings, "Modify Addressable Settings");

            if (needsGroupChange)
            {
                var group = settings.FindGroup(groupName);
                if (group == null)
                {
                    Debug.LogError($"Group '{groupName}' not found");
                    return;
                }

                entry = settings.CreateOrMoveEntry(guid, group, false, !needsLabelChange);
            }

            if (needsLabelChange)
            {
                if (removeOtherLabels)
                {
                    foreach (var existing in entry.labels.ToList())
                        entry.SetLabel(existing, false, true, false);
                }

                entry.SetLabel(labelName, true);
            }
        }

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