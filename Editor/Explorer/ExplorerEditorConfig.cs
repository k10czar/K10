using System;
using System.Collections.Generic;
using System.Linq;
using Rogue.Helpers;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    [Serializable]
    public class ExplorerEditorSaveEntry : IFetchable<Object>
    {
        [field: SerializeField] public Object Key { get; private set; }

        public List<KeyValueConfig<Object, string>> pinned = new();
        public List<KeyValueConfig<Object, string>> favorites = new();
        [SerializeReference] public List<ExplorerSearchConfigBase> searches = new();

        public ExplorerEditorSaveEntry(Object key) => Key = key;

        public void AddFavorite(Object target, string propertyPath)
            => favorites.Add(new KeyValueConfig<Object, string>(target, propertyPath));

        public bool IsFavorite(Object target, string propertyPath)
            => favorites.Any(entry => entry.key == target && entry.value == propertyPath);

        public void RemoveFavorite(Object target, string propertyPath)
            => favorites.RemoveAll(entry => entry.key == target && entry.value == propertyPath);

        public void Pin(Object target, string propertyPath)
            => pinned.Add(new KeyValueConfig<Object, string>(target, propertyPath));

        public bool IsPinned(Object target, string propertyPath)
            => pinned.Any(entry => entry.key == target && entry.value == propertyPath);

        public void RemovePin(Object target, string propertyPath)
            => pinned.RemoveAll(entry => entry.key == target && entry.value == propertyPath);
    }

    public class ExplorerEditorConfig : ScriptableObject
    {
        private const string ConfigPath = "Assets/Custom/Debug/ExplorerEditorConfig.asset";

        private static ExplorerEditorConfig instance;
        public static ExplorerEditorConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabaseUtils.LoadOrCreateSO<ExplorerEditorConfig>(ConfigPath);
                    serializedObjInstance = PropertyCollection.GetSerializedObject(instance);
                }

                return instance;
            }
        }

        private static SerializedObject serializedObjInstance;
        public static SerializedObject SerializedObjInstance
        {
            get
            {
                if (serializedObjInstance == null) _ = Instance;
                return serializedObjInstance;
            }
        }

        public List<ExplorerEditorSaveEntry> explorerSaves = new();

        public static ExplorerEditorSaveEntry GetExplorerSave(Object owner)
        {
            var config = Instance;
            var saveEntry = config.explorerSaves.Fetch(owner);

            if (saveEntry == null)
            {
                saveEntry = new ExplorerEditorSaveEntry(owner);
                Undo.RecordObject(config, "New Save");
                config.explorerSaves.Add(saveEntry);
                PropertyCollection.ApplyDirectChanges(config);
            }

            return saveEntry;
        }

        public static (int, ExplorerEditorSaveEntry) GetExplorerSaveInfo(Object owner)
        {
            var config = Instance;
            var index = config.explorerSaves.FetchIndex(owner);

            return (index, config.explorerSaves[index]);
        }

        public static bool IsFavorite(Object owner, Object target, string propertyPath)
            => GetExplorerSave(owner).IsFavorite(target, propertyPath);

        public static bool IsPinned(Object owner, Object target, string propertyPath)
            => GetExplorerSave(owner).IsPinned(target, propertyPath);
    }
}