using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rogue.REditor
{
    public static class EditorUtils
    {
        public static bool IsEditingData => EditorGUIUtility.editingTextField;

        private static Vector2 lastMousePos;

        public static void CacheMousePos()
        {
            if (Event.current == null)
            {
                Debug.LogError("Calling cache mouse pos on wrong timing!");
                return;
            }

            lastMousePos = Event.current.mousePosition;
        }

        public static Rect GetRectAtMouse()
        {
            if (Event.current != null)
                lastMousePos = Event.current.mousePosition;

            return new Rect(lastMousePos.x, lastMousePos.y, 1, 1);
        }

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

            if (lockInspector) SetFocusedInspectorLock(true);

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

            if (lockInspector) SetFocusedInspectorLock(true);

            Debug.Log("Waiting for Scene click!");

            SceneView.duringSceneGui += wrapper;
        }

        public static void FocusSceneView(Vector3 target, bool hardFocus = true)
        {
            var view = SceneView.lastActiveSceneView;

            view.pivot = target;
            view.size = hardFocus ? .7f : 5;
        }

        #region Inspector Reflections

        public static void SetFocusedInspectorLock(bool shouldLock)
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
                Debug.LogError("Couldn't access isLocked property.");
            }
        }

        public static bool IsFocusedInspectorLocked()
        {
            var focused = EditorWindow.focusedWindow;
            if (focused == null) return false;

            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (focused.GetType() != inspectorType)
            {
                Debug.Log("Focused window is not an Inspector.");
                return false;
            }

            var isLockedProperty = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            if (isLockedProperty == null)
            {
                Debug.LogError("Couldn't access isLocked property.");
                return false;
            }

            return (bool) isLockedProperty.GetValue(focused);
        }

        public static EditorWindow GetInspectorTargeting(Object target)
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var trackerField = inspectorType.GetField("m_Tracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (trackerField == null)
            {
                Debug.LogError("GetTracker method type could not be found.");
                return null;
            }

            var allInspectors = Resources.FindObjectsOfTypeAll(inspectorType);
            foreach (var inspectorWindow in allInspectors.Cast<EditorWindow>())
            {
                if (trackerField.GetValue(inspectorWindow) is not ActiveEditorTracker tracker) continue;

                foreach (var editor in tracker.activeEditors)
                {
                    if (editor.targets.Contains(target))
                        return inspectorWindow;
                }
            }

            return null;
        }

        public static EditorWindow OpenNewInspectorWindow()
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

            var createWindowMethod = typeof(EditorWindow).GetMethod("CreateWindow", new[] { typeof(Type[]) });
            var genericMethod = createWindowMethod!.MakeGenericMethod(inspectorType);

            return (EditorWindow) genericMethod.Invoke(null, new object[] { Type.EmptyTypes });
        }

        #endregion

        #region Console Reflections

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

        public static List<Match> FindConsoleMessages(Regex pattern, bool onlyFirstLine = true)
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
            var matches = new List<Match>();

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
                if (match.Success) matches.Add(match);
            }

            return matches;
        }

        #endregion
    }
}