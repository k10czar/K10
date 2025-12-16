using System;
using System.Linq;
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

        protected void Initialize()
        {
            CacheProperties(false);
            Target = target as T;
        }

        public void DrawOnlyConfigs()
        {
            Initialize();
            using var profilerMarker = drawMarker.Auto();
            DrawConfigs();
        }

        private void DrawConfigsInternal()
        {
            CacheProperties(false);

            using var profilerMarker = drawMarker.Auto();

            DrawScriptFile();
            DrawTitle();
            DrawSaveFile();
            DrawConfigs();
        }

        private bool IsValid()
        {
            if (serializedObject.targetObject == null || target == null) return false;

            if (target is Component component) return component.gameObject != null;
            if (target is GameObject gameObject) return gameObject != null;

            return true;
        }

        public override void OnInspectorGUI()
        {
            if (skipDrawing)
            {
                EditorGUILayout.HelpBox("Changing playmode...", MessageType.Info);
                return;
            }

            if (!IsValid())
            {
                EditorGUILayout.HelpBox("GameObject was destroyed.", MessageType.Warning);
                return;
            }

            Target = target as T;

            if (HasRuntimeVisualization && Application.isPlaying)
            {
                DrawRuntimeInfo();
                SkyxLayout.Space();

                using var scope = HeaderScope.Open("Configs", ref isConfigsExpanded);
                if (scope.IsExpanded) DrawConfigsInternal();
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
            if (reset) PropertyCollection.ScheduleReset(serializedObject);
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

        #region Save Changes

        private bool showModifications;

        protected void DrawSaveFile()
        {
            if (!ShouldDrawSaveFile) return;

            if (EditorUtility.IsDirty(target))
            {
                if (SkyxLayout.Button("Save Changes!", EColor.Warning))
                    PropertyCollection.SaveAsset(target);
            }
            else if (ShouldDrawReserialize)
            {
                if (SkyxLayout.Button("Reserialize", EColor.Special))
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

        protected bool DrawPrefabModifications()
        {
            var targetComponent = Target;
            var targetObject = (targetComponent as Component)!.gameObject;
            if (!PrefabUtility.IsPartOfPrefabInstance(targetObject)) return false;

            var sourceComponent = PrefabUtility.GetCorrespondingObjectFromSource(targetComponent);
            var outerRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(targetObject);
            var nearestRoot = PrefabUtility.GetNearestPrefabInstanceRoot(targetObject);
            var isNestedPrefab = outerRoot != nearestRoot;

            var thisComponentMods = PrefabUtility.GetPropertyModifications(targetObject)
                .Where(entry => entry.target is T && entry.target == sourceComponent)
                .ToArray();

            if (thisComponentMods.Length <= 0) return false;

            TryShowModifications(thisComponentMods);
            SkyxLayout.CompactSpace();

            if (isNestedPrefab)
            {
                if (SkyxLayout.Button($"Apply to {outerRoot.name} Prefab (NESTED)", EColor.Warning))
                    ApplyOverrides(targetComponent, outerRoot);

                if (SkyxLayout.Button($"Apply to {nearestRoot.name} Prefab (PARENT)", EColor.Warning))
                    ApplyOverrides(targetComponent, targetComponent);
            }
            else
            {
                if (SkyxLayout.Button($"Apply Modifications to {nearestRoot.name}", EColor.Warning))
                    ApplyOverrides(targetComponent, targetComponent);
            }

            if (SkyxLayout.Button("Undo Modifications", EColor.Danger))
                UndoOverrides(targetComponent);

            SkyxLayout.Separator();

            return true;
        }

        private static void UndoOverrides(T targetComponent)
        {
            PrefabUtility.RevertObjectOverride(targetComponent, InteractionMode.UserAction);
        }

        private static void ApplyOverrides(T targetComponent, Object pathSource)
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(pathSource);
            PrefabUtility.ApplyObjectOverride(targetComponent, path, InteractionMode.UserAction);
        }

        private void TryShowModifications(PropertyModification[] modifications)
        {
            using var scope = FoldoutScope.Open($"⚠️ {modifications.Length} Modifications!", ref showModifications, EColor.Warning);
            if (!scope.IsExpanded) return;

            foreach (PropertyModification modification in modifications)
                EditorGUILayout.LabelField($"{serializedObject.GetReadablePath(modification.propertyPath)}: {modification.value}");
        }

        #endregion
    }
}