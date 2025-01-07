using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public abstract class InspectorEditor<T> : Editor where T : Object
    {
        protected T Target { get; private set; }
        protected PropertyCollection Properties { get; private set; }

        private bool skipDrawing;

        protected virtual bool ShouldDrawScript => true;
        protected virtual bool ShouldDrawTitle => false;
        protected virtual bool HasRuntimeVisualization => false;
        protected virtual void DrawRuntimeInfo() {}

        private void DrawConfigsInternal()
        {
            DrawScriptFile();
            DrawTitle();
            DrawConfigs();
        }

        protected abstract void DrawConfigs();

        public override void OnInspectorGUI()
        {
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
                CacheProperties();
                DrawConfigsInternal();
                if (PropertyCollection.TryApply(serializedObject)) CacheProperties();
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

        private void CacheProperties() => Properties = PropertyCollection.Get(serializedObject);

        protected void ApplyDirectTargetChanges()
        {
            EditorUtility.SetDirty(Target);
            PropertyCollection.Release(serializedObject);
            CacheProperties();
        }

        protected virtual void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            skipDrawing = playModeStateChange is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode;
        }

        protected virtual void OnEnable()
        {
            Target = target as T;
            CacheProperties();

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