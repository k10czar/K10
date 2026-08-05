using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using K10.DebugSystem;
using Skyx.RuntimeEditor;
using Unity.Profiling;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Rogue.REditor
{
    public class PropertyCollection : ILoggable<EditorDebug>
    {
        #region Static

        private static readonly ProfilerMarker getCollectionMarker = new("PropertyCollection.Get");
        private static readonly ProfilerMarker applyCollectionMarker = new("PropertyCollection.Apply");

        [ResetedOnLoad] private static readonly Dictionary<int, Dictionary<string, PropertyCollection>> collections = new();
        [ResetedOnLoad] private static readonly HashSet<int> scheduledResets = new();
        [ResetedOnLoad] private static readonly Dictionary<int, Action> changedCallbacks = new();

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        public static PropertyCollection Get(SerializedObject root, string path)
        {
            using var profilerMarker = getCollectionMarker.Auto();

            var id = root.GetMainCacheID();
            if (!collections.TryGetValue(id, out var objectCollections))
            {
                objectCollections = new Dictionary<string, PropertyCollection>();
                collections.Add(id, objectCollections);
            }

            if (objectCollections.TryGetValue(path, out var collection)) return collection;

            var isRoot = string.IsNullOrEmpty(path);
            Log($"Creating new collection for {root.targetObject.name} @ {PropertyName(path)}", isRoot ? LogSeverity.Info : LogSeverity.Warning);

            collection = new PropertyCollection(root, path);
            objectCollections.Add(path, collection);

            return collection;
        }

        public static void Apply(SerializedObject serializedObject, string reason)
        {
            using var profilerMarker = applyCollectionMarker.Auto();

            var target = serializedObject.targetObject;

            LogVerbose($"Applying changes to {target.name}: {reason}");

            Undo.RecordObject(target, reason);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);

            ScheduleReset(serializedObject);

            if (target is IPropertyChangedListener listener)
                listener.OnPropertyChanged();
        }

        public static void ApplyDirectChanges(Object target)
        {
            EditorUtility.SetDirty(target);
            ScheduleReset(target.GetInstanceID());
        }

        public static void ScheduleReset(SerializedObject serializedObject)
            => ScheduleReset(serializedObject.GetMainCacheID());

        private static void ScheduleReset(int mainCacheID)
        {
            scheduledResets.Add(mainCacheID);

            if (scheduledResets.Count == 1)
                EditorUtils.RunDelayedOnce(ResetCollections);
        }

        private static void ResetCollections()
        {
            var resets = scheduledResets.ToList();
            scheduledResets.Clear();

            foreach (var mainCacheID in resets)
            {
                SkyxGUI.ClearMyCaches(mainCacheID, true);
                ResetCollections(mainCacheID);

                if (changedCallbacks.TryGetValue(mainCacheID, out var callbacks))
                {
                    try { callbacks(); }
                    catch (Exception exception) { Debug.LogException(exception); }
                }
            }
        }

        private static void ResetCollections(int mainCacheID)
        {
            if (!collections.TryGetValue(mainCacheID, out var objectCollections)) return;

            if (objectCollections.Count == 0)
            {
                collections.Remove(mainCacheID);
                return;
            }

            var first = objectCollections.First().Value;
            if (first.root == null || first.root.targetObject == null)
            {
                collections.Remove(mainCacheID);
                return;
            }

            first.root.Update();

            foreach (var (path, collection) in objectCollections.ToList())
            {
                try
                {

                    // if (collection.IsValid(serializedObject)) continue;

                    collection.Reset();
                    LogVerbose($"Collection {PropertyName(path)} was reset.");

                    continue;
                }
                catch
                {
                    // ignored
                }

                LogVerbose($"Collection for {first.root.targetObject} @ {PropertyName(path)} was corrupted! Deleting...");
                objectCollections.Remove(path);
            }
        }

        public static void Release(SerializedObject root) => Release(root.GetMainCacheID());
        public static void Release(int mainCacheID) => collections.Remove(mainCacheID);

        public static void ClearCollections()
        {
            collections.Clear();
            scheduledResets.Clear();
        }

        public static void RegisterChanged(int mainCacheID, Action callback)
        {
            if (changedCallbacks.TryGetValue(mainCacheID, out var existingCallbacks))
            {
                existingCallbacks -= callback;
                existingCallbacks += callback;

                changedCallbacks[mainCacheID] = existingCallbacks;
            }
            else changedCallbacks[mainCacheID] = callback;
        }

        public static void DeregisterChanged(int mainCacheID, Action callback)
        {
            if (!changedCallbacks.TryGetValue(mainCacheID, out var existingCallbacks)) return;

            existingCallbacks -= callback;

            if (existingCallbacks == null) changedCallbacks.Remove(mainCacheID);
            else changedCallbacks[mainCacheID] = existingCallbacks;
        }

        private static string PropertyName(string path) => string.IsNullOrEmpty(path) ? "_ROOT_" : path;

        #region ExternalChanges

        public static void AssetsChanged()
        {
            Log("Assets changed! Releasing all collections.");
            SkyxGUI.ClearAllCaches();
        }

        private static void OnUndoRedoPerformed()
        {
            LogVerbose("Undo performed!");
            SkyxGUI.ClearAllCaches();
        }

        static PropertyCollection()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        #endregion

        #region Debug

        [MenuItem("Rogue/Editor/Log PropertyCollections")]
        private static void LogPropertyCollections()
        {
            var total = collections.Sum(entry => entry.Value.Count);
            Log($"{collections.Count} serializedObjects tracked, with a total of {total} collections.");
        }

        [HideInCallstack]
        private static void Log(string log, LogSeverity severity = LogSeverity.Info) => K10Log<EditorDebug>.Log(severity, log);

        [HideInCallstack]
        private static void LogVerbose(string log) => K10Log<EditorDebug>.LogVerbose(log);

        #endregion

        #endregion

        private readonly SerializedObject root;
        private readonly Object owner;
        private readonly string propertyPath;

        public SerializedProperty MainProperty { get; private set; }

        private readonly Dictionary<string, SerializedProperty> properties = new();
        public int PropertiesCount => properties.Count;

        public void Apply(string reason) => Apply(root, reason);

        public void ResyncProperties() => root.Update();

        #region Layout Draw

        public void Draw(string propertyName, bool isBacking = false, bool indent = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return;

            if (indent) EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            SkyxLayout.Draw(property);
            if (EditorGUI.EndChangeCheck()) property.Apply();
            if (indent) EditorGUI.indentLevel--;
        }

        public void DrawList(string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var list = GetOrRegisterList(property, displayHeader);
            list.DoLayoutList();
        }

        public void Draw(string propertyName, string label, bool isBacking = false, bool useDefaultDrawer = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return;

            EditorGUI.BeginChangeCheck();
            if (useDefaultDrawer) EditorGUILayout.PropertyField(property, new GUIContent(label));
            else SkyxLayout.Draw(property, label);
            if (EditorGUI.EndChangeCheck()) property.Apply();
        }

        public void DrawIncluding(params string[] including)
        {
            foreach (var entry in including) Draw(entry);
        }

        public void DrawIncluding(params (string, bool)[] including)
        {
            foreach (var (key, isBacking) in including)
                Draw(key, isBacking);
        }

        public void DrawExcept(params string[] except)
        {
            foreach (var key in properties.Keys)
            {
                if (except.Contains(key)) continue;
                Draw(key);
            }
        }

        public void DrawEnum<T>(string propertyName, EColor color = EColor.Primary, string hint = null, bool isMask = false, bool isBacking = false) where T: Enum
        {
            var property = Get(propertyName, isBacking);

            var inner = EditorGUILayout.GetControlRect(false);
            EditorGUI.LabelField(inner.ExtractLabelRect(), property.displayName);

            if (isMask) EnumTreeGUI.DrawEnumMask<T>(inner, property, color, hint);
            else EnumTreeGUI.DrawEnum<T>(inner, property, color, hint);
        }

        #endregion

        #region Rect Draw

        public bool Draw(ref Rect rect, string propertyName, ERectSlideDir slideDir = ERectSlideDir.Vertical, bool drawLabel = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var hasChanged = SkyxGUI.Draw(rect, property, drawLabel);
            rect.Slide(slideDir);

            return hasChanged;
        }

        public void DrawFloat(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).floatValue != 0, inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        public void DrawInt(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).intValue != 0, inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        public void DrawString(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), !string.IsNullOrEmpty(Get(propertyName, isBacking).stringValue), inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        public void DrawCleanString(ref Rect rect, string propertyName, bool isBacking = false, ERectSlideDir slideDir = ERectSlideDir.Vertical)
        {
            var property = Get(propertyName, isBacking);

            EditorGUI.BeginChangeCheck();
            EditorGUI.DelayedTextField(rect, property);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = property.stringValue.Clean();
                property.Apply();
            }

            rect.Slide(slideDir);
        }

        private static void Draw(ref Rect rect, SerializedProperty property, bool hasValue, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool alwaysDrawInlaid = false)
        {
            SkyxGUI.Draw(rect, property);

            SkyxGUI.DrawHintOverlay(ref rect, overlayHint ?? inlaidHint);
            if (alwaysDrawInlaid || !hasValue) SkyxGUI.DrawHindInlaid(rect, inlaidHint);

            if (slideRect) rect.SlideSame();
        }

        public void DrawEnumAndLabel<T>(ref Rect rect, string propertyName, EColor color = EColor.Primary, string label = null, string hint = null, bool isBacking = false) where T: Enum
        {
            var property = Get(propertyName, isBacking);

            var inner = rect;
            EditorGUI.LabelField(inner.ExtractLabelRect(), label ?? property.displayName);
            EnumTreeGUI.DrawEnum<T>(inner, property, color, hint);

            rect.NextSameLine();
        }

        public void DrawEnum<T>(ref Rect rect, string propertyName, EColor color = EColor.Primary, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawEnum<T>(rect, Get(propertyName, isBacking), color, hint);
            if (slideRect) rect.SlideSame();
        }

        public void DrawSwitch<T>(ref Rect rect, string propertyName, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawSwitch<T>(rect, Get(propertyName, isBacking), hint);
            if (slideRect) rect.SlideSame();
        }

        public void DrawEnumMask<T>(ref Rect rect, string propertyName, EColor color = EColor.Primary, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawEnumMask<T>(rect, Get(propertyName, isBacking), color, hint);
            if (slideRect) rect.SlideSame();
        }

        public void DrawObjectField<T>(ref Rect rect, string propertyName, string hint = null, bool allowSceneObjects = false, bool slideRect = true, bool isBacking = false) where T: Object
        {
            SkyxGUI.DrawObjectField<T>(rect, Get(propertyName, isBacking), hint, allowSceneObjects);
            if (slideRect) rect.SlideSame();
        }

        public bool DrawChoiceToggle(ref Rect rect, string propertyName, string onLabel, string offLabel, string hint = null, bool slideRect = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            SkyxGUI.DrawChoiceToggle(rect, onLabel, offLabel, property, hint);

            if (slideRect) rect.SlideSame();

            return property.boolValue;
        }

        public bool DrawSuccessToggle(ref Rect rect, string propertyName, string label = null, string hint = null, bool slideRect = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            SkyxGUI.DrawSuccessToggle(rect, string.IsNullOrEmpty(label) ? property.PrettyName() : label, property, hint);

            if (slideRect) rect.SlideSame();

            return property.boolValue;
        }

        public bool DrawMiniToggle(ref Rect rect, string propertyName, EColor onColor, EColor offColor = EColor.Support, string label = null, string hint = null, bool fromEnd = false, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);

            label = string.IsNullOrEmpty(label) ? property.PrettyName() : label;
            SkyxGUI.MiniToggle(ref rect, property, label, label, hint, onColor.Get(), offColor.Get(), false, fromEnd);

            return property.boolValue;
        }

        public void DrawList(Rect rect, string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var list = GetOrRegisterList(property, displayHeader);
            list.DoList(rect);
        }

        public void DrawIncluding(ref Rect rect, params string[] including)
        {
            foreach (var key in including)
                SkyxGUI.ResetHeightAndDraw(ref rect, properties[key]);
        }

        public void DrawIncludingB(ref Rect rect, params (string, bool)[] including)
        {
            foreach (var (key, isBacking) in including)
                SkyxGUI.ResetHeightAndDraw(ref rect, Get(key, isBacking));
        }

        public void DrawExcept(ref Rect rect, params string[] except)
        {
            foreach (var property in IterateExcluding(except))
                SkyxGUI.ResetHeightAndDraw(ref rect, property);
        }

        public void DrawExceptB(ref Rect rect, params (string, bool)[] except)
        {
            foreach (var property in IterateExcludingB(except))
                SkyxGUI.ResetHeightAndDraw(ref rect, property);
        }

        #endregion

        #region Lists

        public ReorderableList GetOrRegisterList(
            string propertyName,
            bool displayHeader = true,
            bool draggable = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true,
            ReorderableListCache.DrawElement customDrawElement = null,
            Action<SerializedProperty> newElementSetup = null,
            ReorderableListCache.DrawListHeader customHeader = null,
            ReorderableListCache.IsElementHighlighted isElementHighlighted = null,
            bool isBacking = false)
        {
            return GetOrRegisterList(Get(propertyName, isBacking), displayHeader, draggable, displayAddButton, displayRemoveButton, customDrawElement, newElementSetup, customHeader, isElementHighlighted);
        }

        public ReorderableList GetOrRegisterList(
            SerializedProperty property,
            bool displayHeader = true,
            bool draggable = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true,
            ReorderableListCache.DrawElement customDrawElement = null,
            Action<SerializedProperty> newElementSetup = null,
            ReorderableListCache.DrawListHeader customHeader = null,
            ReorderableListCache.IsElementHighlighted isElementHighlighted = null)
        {
            if (ReorderableListCache.TryGet(property, out var list)) return list;

            list = ReorderableListCache.CreateDefaultList(property, displayHeader, draggable, displayAddButton, displayRemoveButton, customDrawElement, newElementSetup, customHeader, isElementHighlighted);
            ReorderableListCache.Add(property, list);

            return list;
        }

        #endregion

        #region Getters

        public float GetTotalHeightIncluding(params string[] fields)
        {
            var total = 0f;

            foreach (var field in fields)
            {
                var property = Get(field, false);
                total += SkyxStyles.ElementsMargin;

                if (ReorderableListCache.TryGet(property, out var list)) total += list.GetHeight();
                else total += EditorGUI.GetPropertyHeight(property, true);
            }

            return total;
        }

        public float GetTotalHeightExcluding(params string[] excludeFields)
        {
            var total = 0f;

            foreach (var (field, property) in properties)
            {
                if (excludeFields.Contains(field)) continue;

                total += SkyxStyles.ElementsMargin;

                if (ReorderableListCache.TryGet(property, out var list)) total += list.GetHeight();
                else total += EditorGUI.GetPropertyHeight(property, true);
            }

            return total;
        }

        public float GetPropertyHeight(string propertyName, bool isBacking = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return 0;

            return ReorderableListCache.TryGet(property, out var list)
                ? list.GetHeight()
                : EditorGUI.GetPropertyHeight(property, true);
        }

        public SerializedProperty this[string key] => properties[key];

        public SerializedProperty Get(string propertyName, bool isBacking) => properties[isBacking ? $"<{propertyName}>k__BackingField" : propertyName];

        public bool TryGet(string propertyName, bool isBacking, out SerializedProperty property)
        {
            if (isBacking) propertyName = $"<{propertyName}>k__BackingField";
            if (properties.TryGetValue(propertyName, out property)) return true;

            this.LogError($"{owner} does not contain {propertyName}");
            return false;
        }

        public int PropertyCountExcluding(params string[] excludeFields) => properties.Keys.Except(excludeFields).Count();

        public IEnumerable<SerializedProperty> IterateExcluding(params string[] except)
        {
            foreach (var key in properties.Keys)
            {
                if (except.Contains(key)) continue;
                yield return properties[key];
            }
        }

        public IEnumerable<SerializedProperty> IterateExcludingB(params (string, bool)[] except)
        {
            var parsedKeys = except.Select(entry => entry.Item2 ? entry.Item1.ToBackingFieldName() : entry.Item1);

            foreach (var key in properties.Keys)
            {
                if (parsedKeys.Contains(key)) continue;
                yield return properties[key];
            }
        }

        #endregion

        #region Setup & Dispose

        private (SerializedProperty, bool) GetRootProperty()
        {
            var fromObject = string.IsNullOrEmpty(propertyPath);
            return (fromObject ? root.GetIterator() : root.FindProperty(propertyPath), fromObject);
        }

        private void Setup()
        {
            bool fromObject;
            (MainProperty, fromObject) = GetRootProperty();

            var iterator = MainProperty.Copy();
            if (!iterator.NextVisible(true)) return;

            var endProperty = MainProperty.Copy();
            if (!fromObject) endProperty.NextVisible(false);
            do
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                if (iterator.name == "m_Script") continue;
                properties.Add(iterator.name, iterator.Copy());
            }
            while (iterator.NextVisible(false));
        }

        private void Reset()
        {
            properties.Clear();
            Setup();
        }

        private PropertyCollection(SerializedObject root, string path)
        {
            this.root = root;

            owner = root.targetObject;
            propertyPath = path;

            root.Update();
            Setup();

            LogOwners = new[] { owner };
        }

        public Object[] LogOwners { get; }

        #endregion
    }
}