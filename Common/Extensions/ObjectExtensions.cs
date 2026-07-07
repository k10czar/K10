using UnityEngine;

namespace K10.Common
{
    public static class ObjectExtensions
    {
        public static bool IsNullOrDestroyed(this Object obj)
        {
            if (ReferenceEquals(obj, null)) return true;
            return obj == null;
        }

        public static string SafeName(this Object obj) => IsNullOrDestroyed(obj) ? "Dead Obj" : obj.name;
        public static string SafeToString(this Object obj) => IsNullOrDestroyed(obj) ? "NULL" : obj.ToString();

        public static T InstantiateLinked<T>(this T prefab) where T : MonoBehaviour
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return (T) UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            #endif

            return Object.Instantiate(prefab);
        }
    }
}