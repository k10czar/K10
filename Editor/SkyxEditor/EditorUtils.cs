using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public static class EditorUtils
    {
        public static void RunDelayedOnce(Action action)
        {
            EditorApplication.CallbackFunction wrapper = null;

            wrapper = () =>
            {
                try { action(); } catch (Exception) {}
                EditorApplication.update -= wrapper;
            };

            EditorApplication.update += wrapper;
        }

        public static void RunOnSceneOnce(Action<SceneView> action)
        {
            Action<SceneView> wrapper = null;

            wrapper = sceneView =>
            {
                try { action(sceneView); } catch (Exception) {}
                SceneView.duringSceneGui -= wrapper;
            };

            SceneView.duringSceneGui += wrapper;
        }

        public static void RunOnSceneOnce(Action action)
        {
            Action<SceneView> wrapper = null;

            wrapper = _ =>
            {
                try { action(); } catch (Exception) {}
                SceneView.duringSceneGui -= wrapper;
            };

            SceneView.duringSceneGui += wrapper;
        }

        public static void RunOnSceneClick<T>(Action<T> callback, int physicsMask, bool lockInspector = true) where T : Component
        {
            Action<SceneView> wrapper = null;

            wrapper = _ =>
            {
                if (Event.current == null || Event.current.type != EventType.MouseDown) return;

                var mousePosition = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                if (!Physics.Raycast(ray, out var hit, 1000, physicsMask))
                {
                    Debug.LogError("No hit found!");
                    SceneView.duringSceneGui -= wrapper;
                    return;
                }

                var selected = hit.collider.GetComponentInChildren<T>();
                if (selected == null) selected = hit.collider.GetComponentInParent<T>();

                if (selected == null)
                {
                    Debug.LogError($"Could not find {typeof(T)} in {hit.collider.name} hierarchy!", hit.collider);
                    SceneView.duringSceneGui -= wrapper;

                    return;
                }

                Event.current.Use();

                callback(selected);
                SceneView.duringSceneGui -= wrapper;
            };

            if (lockInspector) SetInspectorLock(true);

            Debug.Log("Waiting for Scene click!");

            SceneView.duringSceneGui += wrapper;
        }

        public static void RunOnSceneClick(Action<RaycastHit> callback, int physicsMask, bool lockInspector = true)
        {
            Action<SceneView> wrapper = null;

            wrapper = _ =>
            {
                if (Event.current == null || Event.current.type != EventType.MouseDown) return;

                var mousePosition = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                if (!Physics.Raycast(ray, out var hit, 1000, physicsMask))
                {
                    Debug.LogError("No hit found!");
                    SceneView.duringSceneGui -= wrapper;
                    return;
                }

                Event.current.Use();

                callback(hit);
                SceneView.duringSceneGui -= wrapper;
            };

            if (lockInspector) SetInspectorLock(true);

            Debug.Log("Waiting for Scene click!");

            SceneView.duringSceneGui += wrapper;
        }

        public static void SetInspectorLock(bool shouldLock)
        {
            var focused = EditorWindow.focusedWindow;
            if (focused == null) return;

            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (focused.GetType() != inspectorType)
            {
                Debug.Log("Focused window is not an Inspector.");
                return;
            }

            var isLockedProperty = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);

            if (isLockedProperty != null)
            {
                isLockedProperty.SetValue(focused, shouldLock);
                focused.Repaint();
            }
            else
            {
                Debug.LogWarning("Couldn't access isLocked property.");
            }
        }

        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            var clearMethod = logEntries!.GetMethod("Clear");
            clearMethod!.Invoke(new object(), null);
        }
    }
}