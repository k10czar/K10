using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    [Serializable]
    public class ExplorerBatchReserialize<T> : ExplorerBatchExecute<T> where T : Object
    {
        public override string ContentName => "Reserialize Assets";

        protected override bool Run(T dataSource, Dictionary<(Object, string), object> innerProperties, ref string log)
        {
            var path = AssetDatabase.GetAssetPath(dataSource);

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Can't find asset {dataSource} in database!");
                return false;
            }

            AssetDatabase.ForceReserializeAssets(new [] {path}, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            return true;
        }
    }
}