using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    public abstract class InspectorEditor<T> : Editor where T : MonoBehaviour
    {
        protected T Target { get; private set; }
        protected PropertyCollection Properties { get; private set; }

        protected virtual bool HasRuntimeVisualization => false;
        protected virtual void DrawRuntimeInfo() {}

        private void DrawConfigsInternal()
        {
            DrawScriptFile();
            DrawConfigs();
        }

        protected virtual void DrawConfigs() => base.OnInspectorGUI();

        public override void OnInspectorGUI()
        {
            if (HasRuntimeVisualization && Application.isPlaying)
            {
                if (!SkyxLayout.ShouldShowBlock("Runtime", $"{typeof(T)}Runtime")) return;

                DrawRuntimeInfo();

                if (!SkyxLayout.ShouldShowBlock("Configs", $"{typeof(T)}Configs")) return;

                DrawConfigsInternal();
            }
            else
            {
                serializedObject.Update();

                DrawConfigsInternal();

                if (serializedObject.ApplyModifiedProperties())
                    CacheProperties();
            }
        }

        private void DrawScriptFile()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour(Target), typeof(T), false);
            EditorGUI.EndDisabledGroup();

            SkyxLayout.Space();
        }

        private void CacheProperties() => Properties = PropertyCollection.Get(serializedObject, true);

        protected virtual void OnEnable()
        {
            Target = target as T;
            CacheProperties();
        }
    }
}