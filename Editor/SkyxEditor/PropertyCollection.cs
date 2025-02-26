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

        private static string GetID(SerializedObject target) => target.targetObject.GetHashCode().ToString();

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        private static PropertyCollection Get(SerializedObject root, string path)
        {
            using var profilerMarker = getCollectionMarker.Auto();

            var id = GetID(root);
            if (!collections.TryGetValue(id, out var objectCollections))
            {
                objectCollections = new Dictionary<string, PropertyCollection>();
                collections.Add(id, objectCollections);
            }

            if (objectCollections.TryGetValue(path, out var collection)) return collection;

            var isRoot = string.IsNullOrEmpty(path);
            Log($"Creating new collection for {root.targetObject.name} @ {(isRoot ? "_ROOT_" : path)}", isRoot ? LogSeverity.Info : LogSeverity.Warning);

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

            ResetCollections(serializedObject);
        }

        private static void ResetCollections(SerializedObject serializedObject)
        {
            var id = GetID(serializedObject);

            if (!collections.TryGetValue(id, out var objectCollections)) return;

            foreach (var (key, collection) in objectCollections.ToList())
            {
                if (collection.IsValid())
                {
                    if (collection.root == serializedObject) continue;

                    try
                    {
                        collection.Reset(serializedObject);
                        LogVerbose($"Collection {key} was reset.");

                        continue;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                LogVerbose($"Collection for {serializedObject.targetObject} @ {key} was corrupted! Deleting...");
                objectCollections.Remove(key);
            }
        }

        public static void Release(SerializedObject root)
        {
            var id = GetID(root);
            if (!collections.ContainsKey(id)) return;

            Log($"Releasing PropertyCollections for {root.targetObject.name}");

            collections.Remove(id);
        }

        private static bool expectingChange;

        public static void SaveAsset(Object target)
        {
            expectingChange = true;
            AssetDatabase.SaveAssetIfDirty(target);
        }

        public static void AssetsChanged()
        {
            if (!expectingChange)
            {
                Log("Assets changed! Releasing all collections.");
                collections.Clear();
            }

            expectingChange = false;
        }

        private static void OnUndoRedoPerformed()
        {
            LogVerbose("Undo performed!");
            collections.Clear();
        }

        static PropertyCollection()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private static void Log(string log, LogSeverity severity = LogSeverity.Info) => K10Log<EditorLogCategory>.Log(severity, log, verbose: severity is LogSeverity.Warning);
        private static void LogVerbose(string log) => K10Log<EditorLogCategory>.Log(LogSeverity.Warning, log, verbose: true);

        #endregion

        private SerializedObject root;
        private Object owner;
        private readonly string propertyPath;

        private readonly Dictionary<string, SerializedProperty> properties = new();

        #region Layout Draw

        public void Draw(string propertyName)
        {
            if (!TryGet(propertyName, out var property)) return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck()) property.Apply();
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
            if (!TryGetBacking(propertyName, out var property)) return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck()) property.Apply();
        }

        public void Draw(string propertyName, string label)
        {
            if (!TryGet(propertyName, out var property)) return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, new GUIContent(label));
            if (EditorGUI.EndChangeCheck()) property.Apply();
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
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, property, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) property.Apply();
            }

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawFloat(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).floatValue != 0, inlaidHint, overlayHint, slideRect);

        public void DrawInt(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).intValue != 0, inlaidHint, overlayHint, slideRect);

        public void DrawString(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false)
            => Draw(ref rect, Get(propertyName, isBacking), !string.IsNullOrEmpty(Get(propertyName, isBacking).stringValue), inlaidHint, overlayHint, slideRect);

        private static void Draw(ref Rect rect, SerializedProperty property, bool hasValue, string inlaidHint = null, string overlayHint = null, bool slideRect = true)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, property, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) property.Apply();

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

        public bool DrawChoiceToggle(ref Rect rect, string propertyName, string onLabel, string offLabel, string hint = null, bool slideRect = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            SkyxGUI.DrawChoiceToggle(rect, onLabel, offLabel, property, hint);

            if (slideRect) rect.SlideSameRect();

            return property.boolValue;
        }

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

        #region Lists

        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

        public bool HasList(string propertyName, bool isBacking = false) => lists.ContainsKey(Get(propertyName, isBacking));
        public bool HasList(SerializedProperty property) => lists.ContainsKey(property);

        public ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (lists.TryGetValue(property, out var list)) return list;

            Debug.LogError($"ReorderableList for {property} not found!");
            return null;
        }

        public ReorderableList RegisterList(string propertyName, bool displayHeader = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true, ReorderableList.ElementCallbackDelegate customDrawElement = null, Action<SerializedProperty> newElementSetup = null, ReorderableList.HeaderCallbackDelegate customHeader = null, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            return RegisterList(property, displayHeader, draggable, displayAddButton, displayRemoveButton, customDrawElement, newElementSetup, customHeader);
        }

        private ReorderableList RegisterList(SerializedProperty property, bool displayHeader = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true, ReorderableList.ElementCallbackDelegate customDrawElement = null, Action<SerializedProperty> newElementSetup = null, ReorderableList.HeaderCallbackDelegate customHeader = null)
        {
            if (HasList(property)) return null;

            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback,
                onAddCallback = OnAddCallback,
                onRemoveCallback = OnRemoveCallback,
                onReorderCallback = OnReorderCallback,
            };

            lists.Add(property, list);

            return list;

            void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                var innerProp = property.GetArrayElementAtIndex(index);

                if (customDrawElement == null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(rect, innerProp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck()) innerProp.Apply();
                }
                else customDrawElement(rect, index, isActive, isFocused);

                if (rect.TryUseRightClick()) PropertyContextMenu.Open(innerProp);
            }

            float ElementHeightCallback(int index)
            {
                var target = property.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(target, true);
            }

            void DrawHeaderCallback(Rect rect)
            {
                if (customHeader == null) EditorGUI.LabelField(rect, property.PrettyName());
                else customHeader(rect);

                if (rect.TryUseRightClick()) PropertyContextMenu.Open(property);
            }

            void OnAddCallback(ReorderableList thisList)
            {
                var index = thisList.selectedIndices.Count > 0 ? Mathf.Min(property.arraySize, thisList.selectedIndices[0] + 1) : property.arraySize;
                property.InsertArrayElementAtIndex(index);
                newElementSetup?.Invoke(property.GetArrayElementAtIndex(index));
                property.Apply($"New array element: {property.propertyPath}");
            }

            void OnRemoveCallback(ReorderableList thisList)
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(thisList);
                property.Apply($"Remove array element: {property.propertyPath}");
            }

            void OnReorderCallback(ReorderableList thisList)
            {
                property.Apply($"Reorder array element: {property.propertyPath}");
            }
        }

        // index = -1 means at end
        public void InsertArrayElementAtIndex(string propertyName, int index, bool isBacking = false)
        {
            var prop = Get(propertyName, isBacking);
            prop.InsertArrayElementAtIndex(index == -1 ? prop.arraySize : index);
            prop.Apply();
        }

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

        private void Setup()
        {
            root.Update();

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