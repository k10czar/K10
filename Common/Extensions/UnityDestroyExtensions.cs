using UnityEngine;
using System.Collections.Generic;
using K10.Common;

namespace K10.Common
{
    public static class UnityDestroyExtensions
    {
        public static void SafeDestroySelf(this GameObject obj)
        {
            if (obj.IsNullOrDestroyed()) return;

            #if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(obj);
            else
            #endif
                Object.Destroy(obj);
        }

        public static void SafeDestroyGO(this Component component)
        {
            if (component.IsNullOrDestroyed()) return;

            #if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(component.gameObject);
            else
            #endif
                Object.Destroy(component.gameObject);
        }

        public static void DestroyAll(this IEnumerable<GameObject> gameObjects)
        {
            if (gameObjects == null) return;
            foreach (GameObject obj in gameObjects)
            {
                if (obj == null) continue;

                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    GameObject.DestroyImmediate(obj);
                else
                #endif
                    GameObject.Destroy(obj);
            }
        }

        public static void DestroyAllAndClear(this IList<GameObject> gameObjects)
        {
            if (gameObjects == null) return;
            gameObjects.DestroyAll();
            gameObjects.Clear();
        }

        public static void DestroyAll<T>(this IEnumerable<T> components) where T : Component
        {
            if (components == null) return;
            foreach (Component obj in components)
            {
                if (obj == null) continue;

                #if UNITY_EDITOR
                if (!Application.isPlaying) Component.DestroyImmediate(obj);
                else
                #endif
                    Component.Destroy(obj);
            }
        }

        public static void DestroyAllGameObjects<T>(this IEnumerable<T> components) where T : Component
        {
            if (components == null) return;
            foreach (Component component in components)
            {
                if (component == null) continue;

                #if UNITY_EDITOR
                if (!Application.isPlaying) GameObject.DestroyImmediate(component.gameObject);
                else
                #endif
                    GameObject.Destroy(component.gameObject);
            }
        }

        public static void DestroyAllGameObjectsAndClear<T>(this IList<T> components) where T : Component
        {
            if (components == null) return;
            components.DestroyAllGameObjects();
            components.Clear();
        }

        public static void DestroyAllAndClear<T>(this IList<T> components) where T : Component
        {
            if (components == null) return;
            components.DestroyAll();
            components.Clear();
        }
    }
}