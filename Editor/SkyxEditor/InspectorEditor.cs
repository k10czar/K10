using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public abstract class InspectorEditor<T> : Editor where T : Object
    {
        private static readonly ProfilerMarker drawMarker = new("InspectorEditor.Draw");

        protected T Target { get; private set; }
        protected PropertyCollection Properties { get; private set; }

        private bool skipDrawing;
        private bool shouldReset;

        protected virtual bool ShouldDrawScript => true;
        protected virtual bool ShouldDrawTitle => false;
        protected virtual bool ShouldDrawSaveFile => false;
        protected virtual bool HasRuntimeVisualization => false;

        protected virtual void DrawRuntimeInfo() {}
        protected abstract void DrawConfigs();

        private void DrawConfigsInternal()
        {
            Target = target as T;
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

            var dirty = EditorUtility.IsDirty(Target);
            var color = dirty ? Colors.Console.Warning : Colors.Console.Support;
            var text = dirty ? "Save Changes!" : "No Changes";

            using var _ = new BackgroundColorScope(color);

            if (GUILayout.Button(text)) AssetDatabase.SaveAssetIfDirty(Target);

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

            if (HasRuntimeVisualization && Application.isPlaying)
            {
                if (SkyxLayout.ShouldShowBlock("Runtime", $"{typeof(T)}Runtime"))
                {
                    DrawRuntimeInfo();
                    SkyxLayout.Space();
                }
                if (!SkyxLayout.ShouldShowBlock("Configs", $"{typeof(T)}Configs")) DrawConfigsInternal();
            }
            else
            {
                DrawConfigsInternal();
                if (PropertyCollection.TryApply(serializedObject)) CacheProperties(false);
            }
        }

        private void DrawTitle()
        {
            if (!ShouldDrawTitle) return;
            SkyxLayout.DrawTitle(target);
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

        protected void ApplyDirectTargetChanges()
        {
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
            Target = target as T;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (serializedObject.targetObject != null)
                PropertyCollection.Release(serializedObject);
        }
    }
}