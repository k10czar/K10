using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Application = UnityEngine.Application;

namespace K10.DebugSystem
{
    [Serializable]
    public class K10DebugConfig
    {
        public const string SaveKey = "K10DebugConfig.json";

        public List<string> log = new();
        public List<string> verbose = new();
        public List<string> visual = new();

        public bool errors = true;
        public EDebugTargets targets = EDebugTargets.All;

        private List<string> GetCorrespondingList(EDebugType debugType) => debugType switch
        {
            EDebugType.Default => log,
            EDebugType.Verbose => verbose,
            EDebugType.Visual => visual,

            _ => throw new ArgumentOutOfRangeException(nameof(debugType), debugType, null)
        };

        public bool CanDebug(Type categoryType, EDebugType debugType)
        {
            var list = GetCorrespondingList(debugType);
            return list.Contains(categoryType.Name);
        }

        public void ToggleDebug(Type categoryType, EDebugType debugType)
        {
            var list = GetCorrespondingList(debugType);
            var target = categoryType.Name;

            if (!list.Remove(target))
                list.Add(target);

            Save();
        }

        public void SetDebug(Type categoryType, EDebugType debugType, bool value)
        {
            var list = GetCorrespondingList(debugType);
            var target = categoryType.Name;

            if (value)
            {
                if (!list.Contains(target))
                    list.Add(target);
            }
            else list.Remove(target);

            Save();
        }

        public void ToggleDebugTargets()
        {
            var next = ((int) targets + 1) % Enum.GetValues(typeof(EDebugTargets)).Length;
            targets = (EDebugTargets) next;
            Save();
        }

        public void ToggleDebugErrors()
        {
            errors = !errors;
            Save();
        }

        public void Save()
        {
            var path = GetPath();
            FileAdapter.SaveAsUTF8(path, JsonUtility.ToJson(this, true));
        }

        private static string GetPath() => Path.Combine(FileAdapter.persistentDataPath, SaveKey);

        public static K10DebugConfig Load()
        {
            var path = GetPath();

            if (FileAdapter.Exists(path))
            {
                var file = FileAdapter.ReadAsUTF8(path);
                return JsonUtility.FromJson<K10DebugConfig>(file);
            }

            var config = new K10DebugConfig();
            config.Save();

            return config;
        }
    }
}