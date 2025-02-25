using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public abstract class InspectorEditor<T> : Editor where T : Object
    {
        private static readonly ProfilerMarker drawMarker = new("InspectorEditor.Draw");
        private static bool isConfigsExpanded;

        protected T Target { get; private set; }
        protected PropertyCollection Properties { get; private set; }

        private bool skipDrawing;
        private bool shouldReset;
        private bool willMakeDirectChanges;

        protected virtual bool ShouldDrawScript => false;
        protected virtual bool ShouldDrawTitle => false;
        protected virtual bool ShouldDrawSaveFile => false;
        protected virtual bool HasRuntimeVisualization => false;

        protected virtual void DrawRuntimeInfo() {}
        protected abstract void DrawConfigs();

        private void DrawConfigsInternal()
        {
            CacheProperties(false);

            using var profilerMarker = drawMarker.Auto();

            DrawScriptFile();
            DrawTitle();
            DrawSaveFile();
            DrawConfigs();
        }

        private void DrawSaveFile()
        {
            if (!ShouldDrawSaveFile) return;

            var dirty = EditorUtility.IsDirty(target);
            var color = dirty ? Colors.Console.Warning : Colors.Console.Support;
            var text = dirty ? "Save Changes!" : "No Changes";

            using var _ = new BackgroundColorScope(color);

            if (GUILayout.Button(text)) PropertyCollection.SaveAsset(target);

            SkyxLayout.Space();
        }

        private void TryResetDueToExternalChanges()
        {
            if (shouldReset)
            {
                PropertyCollection.Release(serializedObject);
                serializedObject.Update();
                shouldReset = false;
            }

            if (Event.current.type is EventType.ContextClick)
                shouldReset = true;
        }

        public override void OnInspectorGUI()
        {
            TryResetDueToExternalChanges();

            if (skipDrawing)
            {
                EditorGUILayout.HelpBox("Changing playmode...", MessageType.Info);
                return;
            }

            Target = target as T;

            if (HasRuntimeVisualization && Application.isPlaying)
            {
                DrawRuntimeInfo();
                SkyxLayout.Space();

                using var scope = new HeaderScope("Configs", ref isConfigsExpanded);
                if (scope.isExpanded) DrawConfigsInternal();
            }
            else
            {
                DrawConfigsInternal();
            }
        }

        private void DrawTitle()
        {
            if (!ShouldDrawTitle) return;

            if (SkyxLayout.DrawTitle(target))
                EditorGUIUtility.PingObject(Target);
        }

        private void DrawScriptFile()
        {
            if (!ShouldDrawScript) return;

            EditorGUI.BeginDisabledGroup(true);
            var script = Target is MonoBehaviour behaviour ? MonoScript.FromMonoBehaviour(behaviour) : MonoScript.FromScriptableObject(Target as ScriptableObject);
            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), script, typeof(T), false);
            EditorGUI.EndDisabledGroup();

            SkyxLayout.Space();
        }

        private void CacheProperties(bool reset)
        {
            if (reset) PropertyCollection.Release(serializedObject);
            Properties = PropertyCollection.Get(serializedObject);
        }

        protected void PrepareForDirectChanges()
        {
            willMakeDirectChanges = true;
            Undo.RecordObject(target, $"Direct changes on {target.name}");
        }

        protected void ApplyDirectTargetChanges()
        {
            if (!willMakeDirectChanges) Debug.LogError("PrepareForDirectChanges was not called!");
            willMakeDirectChanges = false;

            EditorUtility.SetDirty(Target);
            serializedObject.Update();

            CacheProperties(true);
        }

        protected virtual void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            skipDrawing = playModeStateChange is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode;
        }

        protected virtual void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            try
            {
                if (serializedObject.targetObject != null)
                    PropertyCollection.Release(serializedObject);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}