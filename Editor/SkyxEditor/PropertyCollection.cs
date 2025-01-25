using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using K10.DebugSystem;
using Unity.Profiling;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public class PropertyCollection : ILoggableTarget<EditorLogCategory>
    {
        #region Static

        private static readonly ProfilerMarker getCollectionMarker = new("PropertyCollection.Get");
        private static readonly ProfilerMarker applyCollectionMarker = new("PropertyCollection.Apply");

        [ResetedOnLoad] private static readonly Dictionary<string, Dictionary<string, PropertyCollection>> collections = new();
        [ResetedOnLoad] private static readonly Dictionary<string, Dictionary<string, uint>> previousHashes = new();
        [ResetedOnLoad] private static readonly Dictionary<string, EventType> lastCheckedEvent = new();

        private static string GetID(SerializedObject target) => target.targetObject.GetHashCode().ToString();

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        public static void Release(SerializedObject root)
        {
            var id = GetID(root);
            if (!collections.ContainsKey(id)) return;

            K10Log<EditorLogCategory>.Log($"Releasing PropertyCollections for {root.targetObject}");

            collections.Remove(id);
            previousHashes.Remove(id);
            lastCheckedEvent.Remove(id);
        }

        private static PropertyCollection Get(SerializedObject root, string path)
        {
            using var profilerMarker = getCollectionMarker.Auto();

            var id = GetID(root);
            if (!collections.TryGetValue(id, out var objectCollections))
            {
                objectCollections = new Dictionary<string, PropertyCollection>();
                collections.Add(id, objectCollections);
                previousHashes.Add(id, new Dictionary<string, uint>());
                lastCheckedEvent.Add(id, EventType.Ignore);
            }

            var hashes = previousHashes[id];

            TryApply(root);

            if (objectCollections.TryGetValue(path, out var collection)) return collection;

            var isRoot = string.IsNullOrEmpty(path);
            K10Log<EditorLogCategory>.Log(isRoot ? LogSeverity.Info : LogSeverity.Warning, $"Creating new collection for {root.targetObject} @ {(isRoot ? "_ROOT_" : path)}", verbose: !isRoot);

            collection = new PropertyCollection(root, path);
            objectCollections.Add(path, collection);
            hashes[path] = collection.ContentHash;

            return collection;
        }

        public static bool TryApply(SerializedObject serializedObject)
        {
            using var profilerMarker = applyCollectionMarker.Auto();

            var shouldApply = false;
            var id = GetID(serializedObject);
            var lastEvent = lastCheckedEvent[id];

            if (lastEvent == Event.current.rawType) return false;
            lastCheckedEvent[id] = Event.current.rawType;

            var objectCollections = collections[id];

            foreach (var collection in objectCollections.Values)
            {
                if (!collection.IsValid())
                {
                    Release(serializedObject);
                    return true;
                }

                if (!collection.IsDirty()) continue;

                K10Log<EditorLogCategory>.LogVerbose($"Collection @ '{collection.propertyPath}' was changed!");
                shouldApply = true;
                break;
            }

            if (shouldApply)
            {
                K10Log<EditorLogCategory>.Log($"Applying modifications for {serializedObject.targetObject}");

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                ResetCollections(serializedObject);
            }

            return shouldApply;
        }

        private static void ResetCollections(SerializedObject serializedObject)
        {
            var id = GetID(serializedObject);

            if (!collections.TryGetValue(id, out var objectCollections)) return;
            var hashes = previousHashes[id];

            foreach (var (key, collection) in objectCollections.ToList())
            {
                if (collection.IsValid())
                {
                    if (collection.root == serializedObject)
                    {
                        hashes[key] = collection.ContentHash;
                        continue;
                    }

                    try
                    {
                        collection.Reset(serializedObject);
                        hashes[key] = collection.ContentHash;

                        K10Log<EditorLogCategory>.LogVerbose($"Collection [{key}] reset: {hashes[key]} >>> {collection.ContentHash}");

                        continue;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                K10Log<EditorLogCategory>.LogVerbose($"Collection for {serializedObject.targetObject} @ {key} was corrupted! Deleting...");
                objectCollections.Remove(key);
                hashes.Remove(key);
            }
        }

        #endregion

        private SerializedObject root;
        private Object owner;
        private readonly string propertyPath;

        private readonly Dictionary<string, SerializedProperty> properties = new();

        #region Layout Draw

        public void DrawRelative(string fullPath)
        {
            var paths = fullPath.Split('.');
            if (!properties.TryGetValue(paths[0], out SerializedProperty pathProperty)) return;

            for (int i = 1; i < paths.Length; i++)
                pathProperty = pathProperty.FindPropertyRelative(paths[i]);

            EditorGUILayout.PropertyField(pathProperty);
        }

        public void Draw(string propertyName, int indent = 0)
        {
            if (!TryGet(propertyName, out SerializedProperty property)) return;

            if (indent > 0) EditorGUI.indentLevel += indent;
            EditorGUILayout.PropertyField(property);
            if (indent > 0) EditorGUI.indentLevel -= indent;
        }

        public void DrawList(string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);

            if (!HasList(property)) RegisterList(property, displayHeader);

            var list = GetReorderableList(property);
            list.DoLayoutList();
        }

        public void DrawBacking(string propertyName)
        {
            if (TryGetBacking(propertyName, out var property))
                EditorGUILayout.PropertyField(property);
        }

        public void Draw(string propertyName, string label)
        {
            if (TryGet(propertyName, out var property))
                EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        public void DrawAll(params string[] including)
        {
            foreach (var entry in including) Draw(entry);
        }

        public void DrawAllExcept(params string[] except)
        {
            foreach (var key in properties.Keys)
            {
                if (except.Contains(key)) continue;
                Draw(key);
            }
        }

        #endregion

        #region Rect Draw

        public void Draw(ref Rect rect, string propertyName, bool slideRect = true)
        {
            if (TryGet(propertyName, out var property))
                EditorGUI.PropertyField(rect, property, GUIContent.none);

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawFloat(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).floatValue != 0, inlaidHint, overlayHint, slideRect);

        public void DrawInt(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).intValue != 0, inlaidHint, overlayHint, slideRect);

        public void DrawString(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), string.IsNullOrEmpty(Get(propertyName, isBacking).stringValue), inlaidHint, overlayHint, slideRect);

        private static void Draw(ref Rect rect, SerializedProperty property, bool hasValue, string inlaidHint = null, string overlayHint = null, bool slideRect = true)
        {
            EditorGUI.PropertyField(rect, property, GUIContent.none);

            SkyxGUI.DrawHintOverlay(rect, overlayHint ?? inlaidHint);
            if (!hasValue) SkyxGUI.DrawHindInlaid(rect, inlaidHint);

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawEnum<T>(ref Rect rect, string propertyName, EConsoleColor color = EConsoleColor.Primary, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawEnum<T>(rect, Get(propertyName, isBacking), color, hint);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawSwitch<T>(ref Rect rect, string propertyName, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawSwitch<T>(rect, Get(propertyName, isBacking), hint);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawEnumMask<T>(ref Rect rect, string propertyName, EConsoleColor color = EConsoleColor.Primary, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawEnumMask<T>(rect, Get(propertyName, isBacking), color, hint);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawObjectField<T>(ref Rect rect, string propertyName, string hint = null, bool slideRect = true, bool isBacking = false) where T: Object
        {
            SkyxGUI.DrawObjectField<T>(rect, Get(propertyName, isBacking), hint);
            if (slideRect) rect.SlideSameRect();
        }

        public bool DrawSuccessToggle(ref Rect rect, string propertyName, string onLabel, string offLabel, string hint = null, bool slideRect = true, bool isBacking = false)
            => DrawSuccessToggle(ref rect, propertyName, Get(propertyName, isBacking).boolValue ? onLabel : offLabel, hint, slideRect, isBacking);

        public bool DrawSuccessToggle(ref Rect rect, string propertyName, string label = null, string hint = null, bool slideRect = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            SkyxGUI.DrawSuccessToggle(rect, string.IsNullOrEmpty(label) ? property.PrettyName() : label, property, hint);

            if (slideRect) rect.SlideSameRect();

            return property.boolValue;
        }

        public bool DrawMiniToggle(ref Rect rect, string propertyName, EConsoleColor onColor, EConsoleColor offColor = EConsoleColor.Support, string label = null, string hint = null, bool fromEnd = false, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);

            label = string.IsNullOrEmpty(label) ? property.PrettyName() : label;
            SkyxGUI.MiniToggle(ref rect, property, label, label, hint, Colors.Console.Get(onColor), Colors.Console.Get(offColor), false, fromEnd);

            return property.boolValue;
        }

        public void DrawList(Rect rect, string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);

            if (!HasList(property)) RegisterList(property, displayHeader);

            var list = GetReorderableList(property);
            list.DoList(rect);
        }

        #endregion

        #region Scopes

        public FoldoutBoxScope FoldoutScope(string propertyName, bool isBacking = false, string name = null)
        {
            return TryGet(propertyName, isBacking, out var property)
                ? new FoldoutBoxScope(property, string.IsNullOrEmpty(name) ? property.PrettyName() : name)
                : null;
        }

        public HeaderScope HeaderScope(ref Rect fullRect, string propertyName, bool isBacking = false, string name = null)
        {
            return TryGet(propertyName, isBacking, out var property)
                ? new HeaderScope(ref fullRect, property, string.IsNullOrEmpty(name) ? property.PrettyName() : name)
                : null;
        }

        #endregion

        #region Lists

        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (lists.TryGetValue(property, out var list)) return list;

            Debug.LogError($"ReorderableList for {property} not found!");
            return null;
        }

        public ReorderableList RegisterList(string propertyName, bool displayHeader = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true, ReorderableList.ElementCallbackDelegate customDrawElement = null, ReorderableList.AddCallbackDelegate customAdd = null, string header = null, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            return RegisterList(property, displayHeader, draggable, displayAddButton, displayRemoveButton, customDrawElement, customAdd, header);
        }

        public ReorderableList RegisterList(SerializedProperty property, bool displayHeader = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true, ReorderableList.ElementCallbackDelegate customDrawElement = null, ReorderableList.AddCallbackDelegate customAdd = null, string header = null)
        {
            if (HasList(property)) return null;

            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = customDrawElement ?? DrawElementCallback,
                elementHeightCallback = ElementHeightCallback,
                onAddCallback = customAdd,
            };

            lists.Add(property, list);

            return list;

            void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                var target = property.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, target);
            }

            float ElementHeightCallback(int index)
            {
                var target = property.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(target, true);
            }

            void DrawHeaderCallback(Rect rect) => EditorGUI.LabelField(rect, header ?? property.PrettyName());
        }

        public void RegisterList(string propertyName, ReorderableList list, bool isBacking = false) => lists.TryAdd(Get(propertyName, isBacking), list);

        public bool HasList(string propertyName, bool isBacking = false) => lists.ContainsKey(Get(propertyName, isBacking));
        public bool HasList(SerializedProperty property) => lists.ContainsKey(property);

        #endregion

        #region Getters

        public float GetHeight(params string[] excludeFields)
        {
            var total = 0f;

            foreach (var (field, property) in properties)
            {
                if (excludeFields.Contains(field)) continue;

                total += EditorGUIUtility.standardVerticalSpacing;

                if (lists.TryGetValue(property, out var list)) total += list.GetHeight();
                else total += EditorGUI.GetPropertyHeight(property, true);
            }

            return total;
        }

        public float GetPropertyHeight(string propertyName)
        {
            if (!TryGet(propertyName, out var property)) return 0;

            return lists.TryGetValue(property, out var list)
                ? list.GetHeight()
                : EditorGUI.GetPropertyHeight(property, true);
        }

        public SerializedProperty this[string key] => properties[key];

        public SerializedProperty Get(string propertyName, bool isBacking) => properties[isBacking ? $"<{propertyName}>k__BackingField" : propertyName];

        public bool TryGet(string propertyName, bool isBacking, out SerializedProperty property)
        {
            return isBacking ? TryGetBacking(propertyName, out property) : TryGet(propertyName, out property);
        }

        public bool TryGet(string propertyName, out SerializedProperty property)
        {
            if (properties.TryGetValue(propertyName, out property)) return true;

            this.LogError($"{owner} does not contain {propertyName}");
            return false;
        }

        public bool TryGetBacking(string propertyName, out SerializedProperty property)
        {
            propertyName = $"<{propertyName}>k__BackingField";
            if (properties.TryGetValue(propertyName, out property)) return true;

            this.LogError($"{owner} does not contain {propertyName}");
            return false;
        }

        #endregion

        private bool IsDirty()
        {
            var hashes = previousHashes[GetID(root)];
            var previousHash = hashes.GetValueOrDefault(propertyPath, uint.MaxValue);

            return ContentHash != previousHash || root.hasModifiedProperties;
        }

        private bool IsValid()
        {
            try
            {
                _ = properties.Values.All(entry => entry.isExpanded); // Forces internal Verify call
                return true;
            }
            catch
            {
                return false;
            }
        }

        private uint ContentHash => (string.IsNullOrEmpty(propertyPath) ? root.GetIterator() : root.FindProperty(propertyPath)).contentHash;

        private void Setup()
        {
            var fromObject = string.IsNullOrEmpty(propertyPath);
            var rootProperty = fromObject ? root.GetIterator() : root.FindProperty(propertyPath);

            var currentProperty = rootProperty.Copy();
            if (!currentProperty.NextVisible(true)) return;

            var nextSiblingProperty = rootProperty.Copy();
            if (!fromObject) nextSiblingProperty.NextVisible(false);

            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty)) break;
                properties.Add(currentProperty.name, currentProperty.Copy());
            }
            while (currentProperty.NextVisible(false));
        }

        private void Reset(SerializedObject newRoot)
        {
            properties.Clear();
            lists.Clear();

            root = newRoot;
            owner = root.targetObject;

            Setup();
        }

        private PropertyCollection(SerializedObject root, string path)
        {
            this.root = root;

            owner = root.targetObject;
            propertyPath = path;

            Setup();
        }

        public Object LogTarget => owner;
    }
}