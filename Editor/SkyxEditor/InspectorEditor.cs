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
        private bool willMakeDirectChanges;

        protected virtual bool ShouldDrawScript => false;
        protected virtual bool ShouldDrawTitle => false;
        protected virtual bool ShouldDrawSaveFile => false;
        protected virtual bool ShouldDrawReserialize => false;
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

            if (EditorUtility.IsDirty(target))
            {
                if (SkyxLayout.Button("Save Changes!", EConsoleColor.Warning))
                    PropertyCollection.SaveAsset(target);
            }
            else if (ShouldDrawReserialize)
            {
                if (SkyxLayout.Button("Reserialize", EConsoleColor.Special))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", $"Reserialize {target.name} entries?", "Yes", "No"))
                    {
                        var path = AssetDatabase.GetAssetPath(target);
                        AssetDatabase.ForceReserializeAssets(new [] { path });
                        PropertyCollection.SaveAsset(target);
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("No Changes");
                EditorGUI.EndDisabledGroup();
            }

            SkyxLayout.Space();
        }

        public override void OnInspectorGUI()
        {
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

        protected void CacheProperties(bool reset)
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

        protected void ApplyPropertyChanges(string reason = null) => PropertyCollection.Apply(serializedObject, reason ?? $"Modified {serializedObject}");

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