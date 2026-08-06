using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{

    [Serializable]
    public abstract class ExplorerBatchExecute<T> where T : Object
    {
        protected abstract bool Run(T skillDataSource, Dictionary<string, object> innerProperties, ref string log);

        protected virtual int ReallyRun(Dictionary<T, Dictionary<string, object>> targets)
        {
            var changes = 0;

            foreach (var (entry, innerProperties) in targets)
            {
                try
                {
                    string log = null;
                    if (!Run(entry, innerProperties, ref log)) continue;

                    EditorUtility.SetDirty(entry);
                    Debug.LogWarning(log ?? $"Fixed {entry}", entry);
                    changes++;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Debug.LogError($"Failed while attempting to fix {entry}.\n{changes} entries had already been updated!", entry);
                    return -1;
                }
            }

            return changes;
        }

        public void Run(ExplorerSearchConfig<T> target)
        {
            Debug.Log($"Starting fixture {GetType().Name}");

            var changed = ReallyRun(target.TResults);

            if (changed == -1) Debug.LogError("Fixture run failed!");
            else
            {
                Debug.Log($"Fixture changed {changed} entries!");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}