using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public EDebugTargets targetType = EDebugTargets.All;
        public List<string> targets = new();

        public bool errors = true;

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

        public void ToggleDebugTargets(string target)
        {
            if (string.IsNullOrEmpty(target)) return;

            if (!targets.Remove(target))
                targets.Add(target);

            Save();
        }

        public void ToggleDebugTargetType()
        {
            var next = ((int) targetType + 1) % Enum.GetValues(typeof(EDebugTargets)).Length;
            targetType = (EDebugTargets) next;
            Save();
        }

        public void ToggleDebugErrors()
        {
            errors = !errors;
            Save();
        }

        public async void Save()
        {
            var path = GetPath();

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await File.WriteAllTextAsync(path, JsonUtility.ToJson(this, true));
                    return;
                }
                catch (Exception e)
                {
                    if (i < 4) await Task.Delay(500);
                    else Debug.LogError($"Failed to save! {e}");
                }
            }
        }

        private static string GetPath() => Path.Combine(Application.persistentDataPath, SaveKey);

        public static K10DebugConfig Load()
        {
            var path = GetPath();

            if (File.Exists(path))
            {
                var file = File.OpenText(path);
                var data = JsonUtility.FromJson<K10DebugConfig>(file.ReadToEnd());
                file.Close();

                return data;
            }

            var config = new K10DebugConfig();
            config.Save();

            return config;
        }
    }
}