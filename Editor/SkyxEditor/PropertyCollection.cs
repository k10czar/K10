using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Threading.Tasks;
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

        private static readonly ProfilerMarker _getCollectionMarker = new("PropertyCollection.Get");
        private static readonly ProfilerMarker _applyCollectionMarker = new("PropertyCollection.Apply");

        private static readonly Dictionary<EntityId, Dictionary<string, PropertyCollection>> _collections = new();
        private static readonly HashSet<EntityId> _scheduledResets = new();
        private static readonly Dictionary<EntityId, Action> _changedCallbacks = new();

        public static SerializedObject GetSerializedObject(Object target)
        {
            var mainCacheID = target.GetEntityId();
            if (_collections.TryGetValue(mainCacheID, out var objectCollections))
            {
                if (objectCollections.Count > 0)
                    return objectCollections.First().Value._root;
            }

            return new SerializedObject(target);
        }

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        public static PropertyCollection Get(SerializedObject root, string path)
        {
            using var profilerMarker = _getCollectionMarker.Auto();

            var id = root.GetMainCacheID();
            if (!_collections.TryGetValue(id, out var objectCollections))
            {
                objectCollections = new Dictionary<string, PropertyCollection>();
                _collections.Add(id, objectCollections);
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
            using var profilerMarker = _applyCollectionMarker.Auto();

            var target = serializedObject.targetObject;

            LogVerbose($"Applying changes to {target.name}: {reason}");

            Undo.RecordObject(target, reason);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);

            ScheduleReset(serializedObject);
        }

        public static void ApplyDirectChanges(Object target)
        {
            EditorUtility.SetDirty(target);
            ScheduleReset(target.GetEntityId());
        }

        public static void ScheduleReset(SerializedObject serializedObject)
            => ScheduleReset(serializedObject.GetMainCacheID());

        private static void ScheduleReset(EntityId mainCacheID)
        {
            _scheduledResets.Add(mainCacheID);

            if (_scheduledResets.Count == 1)
                EditorUtils.RunDelayedOnce(ResetCollections);
        }

        private static async void ResetCollections()
        {
            try
            {
                await Task.Delay(50);

                var resets = _scheduledResets.ToList();
                _scheduledResets.Clear();

                foreach (var mainCacheID in resets)
                {
                    SkyxGUI.ClearMyCaches(mainCacheID, true);
                    ResetCollections(mainCacheID);

                    if (_changedCallbacks.TryGetValue(mainCacheID, out var callbacks))
                    {
                        try { callbacks(); }
                        catch (Exception exception) { Debug.LogException(exception); }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void ResetCollections(EntityId mainCacheID)
        {
            if (!_collections.TryGetValue(mainCacheID, out var objectCollections)) return;

            if (objectCollections.Count == 0)
            {
                _collections.Remove(mainCacheID);
                return;
            }

            var first = objectCollections.First().Value;
            if (first._root == null || first._root.targetObject == null)
            {
                _collections.Remove(mainCacheID);
                return;
            }

            first._root.Update();

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

                LogVerbose($"Collection for {first._root.targetObject} @ {PropertyName(path)} was corrupted! Deleting...");
                objectCollections.Remove(path);
            }
        }

        public static void Release(SerializedObject root) => Release(root.GetMainCacheID());
        public static void Release(EntityId mainCacheID) => _collections.Remove(mainCacheID);

        public static void ClearCollections()
        {
            _collections.Clear();
            _scheduledResets.Clear();
            _changedCallbacks.Clear();
        }

        public static void RegisterChanged(EntityId mainCacheID, Action callback)
        {
            if (_changedCallbacks.TryGetValue(mainCacheID, out var existingCallbacks))
            {
                existingCallbacks -= callback;
                existingCallbacks += callback;

                _changedCallbacks[mainCacheID] = existingCallbacks;
            }
            else _changedCallbacks[mainCacheID] = callback;
        }

        public static void DeregisterChanged(EntityId mainCacheID, Action callback)
        {
            if (!_changedCallbacks.TryGetValue(mainCacheID, out var existingCallbacks)) return;

            existingCallbacks -= callback;

            if (existingCallbacks == null) _changedCallbacks.Remove(mainCacheID);
            else _changedCallbacks[mainCacheID] = existingCallbacks;
        }

        private static string PropertyName(string path) => string.IsNullOrEmpty(path) ? "_ROOT_" : path;

        #region Debug

        [MenuItem("Rogue/Editor/Log PropertyCollections")]
        private static void LogPropertyCollections()
        {
            var total = _collections.Sum(entry => entry.Value.Count);
            Log($"{_collections.Count} serializedObjects tracked, with a total of {total} collections.");
        }

        [HideInCallstack]
        private static void Log(string log, LogSeverity severity = LogSeverity.Info) => K10Log<EditorDebug>.Log(severity, log);

        [HideInCallstack]
        private static void LogVerbose(string log) => K10Log<EditorDebug>.LogVerbose(log);

        #endregion

        #endregion

        private readonly SerializedObject _root;
        private readonly Object _owner;
        private readonly string _propertyPath;

        public SerializedProperty MainProperty { get; private set; }

        private readonly Dictionary<string, SerializedProperty> _properties = new();
        public int PropertiesCount => _properties.Count;

        public void Apply(string reason) => Apply(_root, reason);

        public void ResyncProperties() => _root.Update();

        #region Layout Draw

        public void Draw(string propertyName, bool isBacking = false, bool indent = false)
        {
            if (!TryGet(propertyName, isBacking, out var property))
                return;

            if (indent) EditorGUI.indentLevel++;

            if (property.IsIList())
            {
                var list = GetOrRegisterList(property);
                list.DoLayoutList();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
                if (EditorGUI.EndChangeCheck()) property.Apply();
            }

            if (indent) EditorGUI.indentLevel--;
        }

        public void DrawList(string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var list = GetOrRegisterList(property, displayHeader);
            list.DoLayoutList();
        }

        public void DrawIncluding(params string[] including)
        {
            foreach (var entry in including)
                Draw(entry);
        }

        public void DrawIncluding(params (string, bool)[] including)
        {
            foreach (var (key, isBacking) in including)
                Draw(key, isBacking);
        }

        public void DrawExcept(params string[] except)
        {
            foreach (var key in _properties.Keys)
            {
                if (except.Contains(key))
                    continue;

                Draw(key);
            }
        }

        #endregion

        #region Rect Draw

        public bool Draw(ref Rect rect, string propertyName, ERectSlideDir slideDir, bool drawLabel = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var hasChanged = SkyxGUI.Draw(rect, property, drawLabel);
            rect.Slide(slideDir);

            return hasChanged;
        }

        public void DrawList(Rect rect, string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            var list = GetOrRegisterList(property, displayHeader);
            list.DoList(rect);
        }

        private void ResetAndDraw(ref Rect rect, SerializedProperty property)
        {
            if (property.IsIList())
            {
                var list = GetOrRegisterList(property);
                rect.height = list.GetHeight();
                list.DoList(rect);
                rect.y += rect.height + SkyxStyles.ElementsMargin;
            }
            else
                SkyxGUI.ResetHeightAndDraw(ref rect, property);
        }

        public void DrawIncluding(ref Rect rect, params string[] including)
        {
            foreach (var key in including)
                ResetAndDraw(ref rect, _properties[key]);
        }

        public void DrawIncludingB(ref Rect rect, params (string, bool)[] including)
        {
            foreach (var (key, isBacking) in including)
                ResetAndDraw(ref rect, Get(key, isBacking));
        }

        public void DrawExcept(ref Rect rect, params string[] except)
        {
            foreach (var property in IterateExcluding(except))
                ResetAndDraw(ref rect, property);
        }

        public void DrawExceptB(ref Rect rect, params (string, bool)[] except)
        {
            foreach (var property in IterateExcludingB(except))
                ResetAndDraw(ref rect, property);
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
            if (ReorderableListCache.TryGet(property, out var list))
                return list;

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

            foreach (var (field, property) in _properties)
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

        public SerializedProperty this[string key] => _properties[key];

        public SerializedProperty Get(string propertyName, bool isBacking) => _properties[isBacking ? $"<{propertyName}>k__BackingField" : propertyName];

        public bool TryGet(string propertyName, bool isBacking, out SerializedProperty property)
        {
            if (isBacking) propertyName = propertyName.ToBackingFieldName();
            return _properties.TryGetValue(propertyName, out property);
        }

        public int PropertyCountExcluding(params string[] excludeFields) => _properties.Keys.Except(excludeFields).Count();

        public IEnumerable<SerializedProperty> IterateExcluding(params string[] except)
        {
            foreach (var key in _properties.Keys)
            {
                if (except.Contains(key)) continue;
                yield return _properties[key];
            }
        }

        public IEnumerable<SerializedProperty> IterateExcludingB(params (string, bool)[] except)
        {
            var parsedKeys = except.Select(entry => entry.Item2 ? entry.Item1.ToBackingFieldName() : entry.Item1);

            foreach (var key in _properties.Keys)
            {
                if (parsedKeys.Contains(key)) continue;
                yield return _properties[key];
            }
        }

        #endregion

        #region Setup & Dispose

        private (SerializedProperty, bool) GetRootProperty()
        {
            var fromObject = string.IsNullOrEmpty(_propertyPath);
            return (fromObject ? _root.GetIterator() : _root.FindProperty(_propertyPath), fromObject);
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
                _properties.Add(iterator.name, iterator.Copy());
            }
            while (iterator.NextVisible(false));
        }

        private void Reset()
        {
            _properties.Clear();
            Setup();
        }

        private PropertyCollection(SerializedObject root, string path)
        {
            this._root = root;

            _owner = root.targetObject;
            _propertyPath = path;

            root.Update();
            Setup();

            LogOwners = new[] { _owner };
        }

        public Object[] LogOwners { get; }

        #endregion
    }
}