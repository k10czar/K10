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
    public class PropertyCollection : ILoggable<EditorDebugCategory>
    {
        #region Static

        private static readonly ProfilerMarker getCollectionMarker = new("PropertyCollection.Get");
        private static readonly ProfilerMarker applyCollectionMarker = new("PropertyCollection.Apply");

        [ResetedOnLoad] private static readonly Dictionary<string, Dictionary<string, PropertyCollection>> collections = new();
        [ResetedOnLoad] private static readonly HashSet<SerializedObject> scheduledResets = new();

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        private static PropertyCollection Get(SerializedObject root, string path)
        {
            using var profilerMarker = getCollectionMarker.Auto();

            var id = root.GetCacheID();
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

        public static void ScheduleReset(SerializedObject serializedObject)
        {
            scheduledResets.Add(serializedObject);

            if (scheduledResets.Count == 1)
                EditorUtils.RunDelayedOnce(ResetCollections);
        }

        private static void ResetCollections()
        {
            foreach (var serializedObject in scheduledResets)
            {
                serializedObject.InvalidateTypeCache();
                ResetCollections(serializedObject);
            }

            scheduledResets.Clear();
        }

        private static void ResetCollections(SerializedObject serializedObject)
        {
            var id = serializedObject.GetCacheID();
            if (!collections.TryGetValue(id, out var objectCollections)) return;

            serializedObject.Update();

            foreach (var (path, collection) in objectCollections.ToList())
            {
                try
                {
                    // if (collection.IsValid(serializedObject)) continue;

                    collection.Reset(serializedObject);
                    LogVerbose($"Collection {PropertyName(path)} was reset.");

                    continue;
                }
                catch
                {
                    // ignored
                }

                LogVerbose($"Collection for {serializedObject.targetObject} @ {PropertyName(path)} was corrupted! Deleting...");
                objectCollections.Remove(path);
            }
        }

        public static void Release(SerializedObject root)
        {
            var id = root.GetCacheID();
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
            scheduledResets.Clear();
        }

        static PropertyCollection()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        [MenuItem("Disyphus/Editor/Clear PropertyCollections")]
        public static void ClearCollections()
        {
            collections.Clear();
            scheduledResets.Clear();
        }

        [MenuItem("Disyphus/Editor/Log PropertyCollections")]
        private static void LogPropertyCollections()
        {
            var total = collections.Sum(entry => entry.Value.Count);
            Log($"{collections.Count} serializedObjects tracked, with a total of {total} collections.");
        }

        private static string PropertyName(string path) => string.IsNullOrEmpty(path) ? "_ROOT_" : path;

        [HideInCallstack]
        private static void Log(string log, LogSeverity severity = LogSeverity.Info) => K10Log<EditorDebugCategory>.Log(severity, log);

        [HideInCallstack]
        private static void LogVerbose(string log) => K10Log<EditorDebugCategory>.LogVerbose(log);

        #endregion

        private SerializedObject root;
        private SerializedProperty rootProperty;
        private Object owner;
        private readonly string propertyPath;

        private readonly Dictionary<string, SerializedProperty> properties = new();
        public int PropertiesCount => properties.Count;

        #region Layout Draw

        public void Draw(string propertyName, bool isBacking = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return;

            EditorGUI.BeginChangeCheck();
            SkyxLayout.Draw(property);
            if (EditorGUI.EndChangeCheck()) property.Apply();
        }

        public void DrawList(string propertyName, bool displayHeader = true, bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);

            if (!HasList(property)) RegisterList(property, displayHeader);

            var list = GetReorderableList(property);
            list.DoLayoutList();
        }

        public void Draw(string propertyName, string label, bool isBacking = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return;

            EditorGUI.BeginChangeCheck();
            SkyxLayout.Draw(property, label);
            if (EditorGUI.EndChangeCheck()) property.Apply();
        }

        public void DrawIncluding(params string[] including)
        {
            foreach (var entry in including) Draw(entry);
        }

        public void DrawExcept(params string[] except)
        {
            foreach (var key in properties.Keys)
            {
                if (except.Contains(key)) continue;
                Draw(key);
            }
        }

        #endregion

        #region Rect Draw

        public void Draw(ref Rect rect, string propertyName, bool slideRect = true, bool isBacking = false, bool drawLabel = false)
        {
            var property = Get(propertyName, isBacking);
            SkyxGUI.Draw(rect, property, drawLabel);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawFloat(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).floatValue != 0, inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        public void DrawInt(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), Get(propertyName, isBacking).intValue != 0, inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        public void DrawString(ref Rect rect, string propertyName, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool isBacking = false, bool alwaysDrawInlaid = false)
            => Draw(ref rect, Get(propertyName, isBacking), !string.IsNullOrEmpty(Get(propertyName, isBacking).stringValue), inlaidHint, overlayHint, slideRect, alwaysDrawInlaid);

        private static void Draw(ref Rect rect, SerializedProperty property, bool hasValue, string inlaidHint = null, string overlayHint = null, bool slideRect = true, bool alwaysDrawInlaid = false)
        {
            SkyxGUI.Draw(rect, property);

            SkyxGUI.DrawHintOverlay(ref rect, overlayHint ?? inlaidHint);
            if (alwaysDrawInlaid || !hasValue) SkyxGUI.DrawHindInlaid(rect, inlaidHint);

            if (slideRect) rect.SlideSameRect();
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
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawSwitch<T>(ref Rect rect, string propertyName, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawSwitch<T>(rect, Get(propertyName, isBacking), hint);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawEnumMask<T>(ref Rect rect, string propertyName, EColor color = EColor.Primary, string hint = null, bool slideRect = true, bool isBacking = false) where T: Enum
        {
            EnumTreeGUI.DrawEnumMask<T>(rect, Get(propertyName, isBacking), color, hint);
            if (slideRect) rect.SlideSameRect();
        }

        public void DrawObjectField<T>(ref Rect rect, string propertyName, string hint = null, bool allowSceneObjects = false, bool slideRect = true, bool isBacking = false) where T: Object
        {
            SkyxGUI.DrawObjectField<T>(rect, Get(propertyName, isBacking), hint, allowSceneObjects);
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

            if (!HasList(property)) RegisterList(property, displayHeader);

            var list = GetReorderableList(property);
            list.DoList(rect);
        }

        public void DrawIncluding(ref Rect rect, params string[] including)
        {
            foreach (var key in including)
            {
                var target = properties[key];

                rect.height = EditorGUI.GetPropertyHeight(target, true);
                SkyxGUI.Draw(rect, target, true);
                rect.y += rect.height + SkyxStyles.ElementsMargin;
            }
        }

        public void DrawExcept(ref Rect rect, params string[] except)
        {
            foreach (var key in properties.Keys)
            {
                if (except.Contains(key)) continue;

                var target = properties[key];

                rect.height = EditorGUI.GetPropertyHeight(target, true);
                SkyxGUI.Draw(rect, target, true);
                rect.y += rect.height + SkyxStyles.ElementsMargin;
            }
        }

        #endregion

        #region Lists

        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

        public delegate bool IsElementHighlighted(SerializedProperty elementProperty);

        public bool HasList(string propertyName, bool isBacking = false) => lists.ContainsKey(Get(propertyName, isBacking));
        public bool HasList(SerializedProperty property) => lists.ContainsKey(property);

        public ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (lists.TryGetValue(property, out var list)) return list;

            Debug.LogError($"ReorderableList for {property} not found!");
            return null;
        }

        public ReorderableList RegisterList(
            string propertyName,
            bool displayHeader = true,
            bool draggable = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true,
            ReorderableList.ElementCallbackDelegate customDrawElement = null,
            Action<SerializedProperty> newElementSetup = null,
            ReorderableList.HeaderCallbackDelegate customHeader = null,
            IsElementHighlighted isElementHighlighted = null,
            bool isBacking = false)
        {
            var property = Get(propertyName, isBacking);
            return RegisterList(property, displayHeader, draggable, displayAddButton, displayRemoveButton, customDrawElement, newElementSetup, customHeader, isElementHighlighted);
        }

        private ReorderableList RegisterList(
            SerializedProperty property,
            bool displayHeader = true,
            bool draggable = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true,
            ReorderableList.ElementCallbackDelegate customDrawElement = null,
            Action<SerializedProperty> newElementSetup = null,
            ReorderableList.HeaderCallbackDelegate customHeader = null,
            IsElementHighlighted isElementHighlighted = null)
        {
            if (lists.TryGetValue(property, out var list)) return list;

            list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback,
                onAddCallback = OnAddCallback,
                onRemoveCallback = OnRemoveCallback,
                onReorderCallback = OnReorderCallback,
                drawElementBackgroundCallback = DrawElementBackgroundCallback,
            };

            if (!displayAddButton && !displayRemoveButton)
                list.footerHeight = 0;

            lists.Add(property, list);

            return list;

            void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0) ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, draggable);

                else
                {
                    var color = isElementHighlighted?.Invoke(property.GetArrayElementAtIndex(index)) ?? false
                        ? (isActive ? Colors.Console.SpecialBackgroundVar : Colors.Console.SpecialBackground)
                        : (isFocused ? Colors.CeruleanBlue : (index % 2 == 0 ? Colors.Console.Dark: Colors.Console.DarkerDark));

                    EditorGUI.DrawRect(rect, color);
                }
            }

            void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                var innerProp = property.GetArrayElementAtIndex(index);

                PropertyContextMenu.ContextGUI(ref rect, innerProp, newElementSetup);

                if (customDrawElement == null) SkyxGUI.Draw(rect, innerProp);
                else customDrawElement(rect, index, isActive, isFocused);
            }

            float ElementHeightCallback(int index)
            {
                var target = property.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(target, true);
            }

            void DrawHeaderCallback(Rect rect)
            {
                PropertyContextMenu.ContextGUI(ref rect, property, newElementSetup);

                if (customHeader == null) EditorGUI.LabelField(rect, property.PrettyName());
                else customHeader(rect);
            }

            void OnAddCallback(ReorderableList thisList)
            {
                var index = thisList.selectedIndices.Count > 0 ? Mathf.Min(property.arraySize, thisList.selectedIndices[0] + 1) : property.arraySize;
                property.InsertArrayElementAtIndex(index);
                property.Apply($"New array element: {property.propertyPath}");

                var newElement = property.GetArrayElementAtIndex(index);
                newElement.ResetDefaultValues(newElementSetup, false);
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
        public void InsertArrayElementAtIndex(string propertyName, int index, bool isBacking = false, Action<SerializedProperty> newElementSetup = null)
        {
            var prop = Get(propertyName, isBacking);
            index = index == -1 ? prop.arraySize : index;
            prop.InsertArrayElementAtIndex(index);
            prop.Apply();

            var newElement = prop.GetArrayElementAtIndex(index);
            newElement.ResetDefaultValues(newElementSetup, false);
        }

        #endregion

        #region Getters

        public float GetTotalHeightExcluding(params string[] excludeFields)
        {
            var total = 0f;

            foreach (var (field, property) in properties)
            {
                if (excludeFields.Contains(field)) continue;

                total += SkyxStyles.ElementsMargin;

                if (lists.TryGetValue(property, out var list)) total += list.GetHeight();
                else total += EditorGUI.GetPropertyHeight(property, true);
            }

            return total;
        }

        public float GetPropertyHeight(string propertyName, bool isBacking = false)
        {
            if (!TryGet(propertyName, isBacking, out var property)) return 0;

            return lists.TryGetValue(property, out var list)
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

        #endregion

        #region Setup & Dispose

        private (SerializedProperty, bool) GetRootProperty()
        {
            var fromObject = string.IsNullOrEmpty(propertyPath);
            return (fromObject ? root.GetIterator() : root.FindProperty(propertyPath), fromObject);
        }

        private bool IsValid(SerializedObject serializedObject)
        {
            return true;
            // TODO: Find valid way to check is things changed
            // if (root != serializedObject) return false;

            // var (targetProperty, fromObject) = GetRootProperty();
            //
            // if (targetProperty.IsManagedRef())
            //     return rootProperty.managedReferenceId == targetProperty.managedReferenceId;
            //
            // return SerializedProperty.EqualContents(targetProperty, rootProperty);

            // try
            // {
            //     _ = properties.Values.All(entry => entry.isExpanded); // Forces internal Verify call
            //     return true;
            // }
            // catch
            // {
            //     return false;
            // }
        }

        private void Setup()
        {
            bool fromObject;
            (rootProperty, fromObject) = GetRootProperty();

            var iterator = rootProperty.Copy();
            if (!iterator.NextVisible(true)) return;

            var endProperty = rootProperty.Copy();
            if (!fromObject) endProperty.NextVisible(false);
            do
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                if (iterator.name == "m_Script") continue;
                properties.Add(iterator.name, iterator.Copy());
            }
            while (iterator.NextVisible(false));
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

            root.Update();
            Setup();

            LogOwners = new[] { owner };
        }

        public Object[] LogOwners { get; }

        #endregion
    }
}