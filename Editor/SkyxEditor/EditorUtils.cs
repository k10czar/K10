using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public static class EditorUtils
    {
        public static bool IsEditingData
            => EditorGUIUtility.editingTextField && EditorWindow.focusedWindow?.GetType().Name == "InspectorWindow";

        public static void RunDelayedOnce(Action action)
        {
            EditorApplication.CallbackFunction wrapper = null;

            wrapper = () =>
            {
                try { action(); }
                catch (Exception e) { Debug.LogException(e); }

                EditorApplication.update -= wrapper;
            };

            EditorApplication.update += wrapper;
        }

        public static void RunFor(Action<float> action, float duration, float loops = 1)
        {
            var startTime = EditorApplication.timeSinceStartup;
            var completedLoops = 0;
            EditorApplication.CallbackFunction wrapper = null;

            wrapper = () =>
            {
                try
                {
                    var elapsed = EditorApplication.timeSinceStartup - startTime;

                    if (elapsed > duration)
                    {
                        completedLoops++;

                        if (completedLoops > loops)
                        {
                            action(1);
                            EditorApplication.update -= wrapper;
                            return;
                        }

                        startTime += duration;
                        elapsed = EditorApplication.timeSinceStartup - startTime;
                    }

                    action((float)(elapsed / duration));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    EditorApplication.update -= wrapper;
                }
            };

            EditorApplication.update += wrapper;
        }

        public static void RunOnSceneOnce(Action<SceneView> action)
        {
            Action<SceneView> wrapper = null;

            wrapper = sceneView =>
            {
                try { action(sceneView); }
                catch (Exception e) { Debug.LogException(e); }
                SceneView.duringSceneGui -= wrapper;
            };

            SceneView.duringSceneGui += wrapper;
        }

        public static void RunOnSceneOnce(Action action)
        {
            Action<SceneView> wrapper = null;

            wrapper = _ =>
            {
                try { action(); }
                catch (Exception e) { Debug.LogException(e); }
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

        public static void FocusSceneView(Vector3 target, bool hardFocus = true)
        {
            var view = SceneView.lastActiveSceneView;

            view.pivot = target;
            view.size = hardFocus ? .7f : 5;
        }

        #region Reflections

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

        public static string GetSelectedConsoleMessage(bool onlyFirstLine = true)
        {
            var consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow, UnityEditor");
            if (consoleWindowType == null) return string.Empty;

            var consoleWindow = EditorWindow.GetWindow(consoleWindowType, false, "Console", false);
            if (consoleWindow == null) return string.Empty;

            var activeTextField = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);

            var line = (string) activeTextField?.GetValue(consoleWindow) ?? string.Empty;
            if (onlyFirstLine) line = line.Split(new[]{'\r','\n'}, 2, StringSplitOptions.None)[0];

            return line;
        }

        public static Match FindConsoleMessage(Regex pattern, bool onlyFirstLine = true)
        {
            var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor");

            if (logEntriesType == null || logEntryType == null)
                return null;

            var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
            var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);

            if (getCountMethod == null || getEntryMethod == null || messageField == null)
                return null;

            var count = (int) getCountMethod.Invoke(null, null);

            for (var i = 0; i < count; i++)
            {
                var entry = Activator.CreateInstance(logEntryType);

                var parameters = new[] { i, entry };
                var result = (bool)getEntryMethod.Invoke(null, parameters);
                if (!result) continue;

                var message = (string) messageField.GetValue(entry);
                if (string.IsNullOrEmpty(message)) continue;

                if (onlyFirstLine) message = message.Split(new[]{'\r','\n'}, 2, StringSplitOptions.None)[0];

                var match = pattern.Match(message);
                if (match.Success) return match;
            }

            return null;
        }

        #endregion
    }
}