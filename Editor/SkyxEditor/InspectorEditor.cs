using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public abstract class InspectorEditor<T> : Editor where T : Object
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

        private void DrawScriptFile()
        {
            EditorGUI.BeginDisabledGroup(true);
            var script = Target is MonoBehaviour behaviour ? MonoScript.FromMonoBehaviour(behaviour) : MonoScript.FromScriptableObject(Target as ScriptableObject);
            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), script, typeof(T), false);
            EditorGUI.EndDisabledGroup();

            SkyxLayout.Space();
        }

        private void CacheProperties() => Properties = PropertyCollection.Get(serializedObject);

        protected virtual void OnEnable()
        {
            Target = target as T;
            CacheProperties();
        }

        protected void OnDisable()
        {
            Target = target as T;

            if (serializedObject.targetObject != null)
                PropertyCollection.Release(serializedObject);
        }
    }
}