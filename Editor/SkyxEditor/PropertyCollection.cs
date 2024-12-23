using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public class PropertyCollection
    {
        #region Static

        [ResetedOnLoad] private static readonly Dictionary<string, Dictionary<string, PropertyCollection>> collections = new();

        private static string GetID(SerializedObject target) => target.targetObject.GetHashCode().ToString();

        public static PropertyCollection Get(SerializedObject serializedObject) => Get(serializedObject, "");
        public static PropertyCollection Get(SerializedProperty property) => Get(property.serializedObject, property.propertyPath);

        public static void Release(SerializedObject root) => collections.Remove(GetID(root));

        private static PropertyCollection Get(SerializedObject root, string path)
        {
            var id = GetID(root);
            if (!collections.TryGetValue(id, out var objectCollections))
            {
                objectCollections = new Dictionary<string, PropertyCollection>();
                collections.Add(id, objectCollections);
            }

            TryApply(root);

            if (objectCollections.TryGetValue(path, out var collection))
            {
                if (collection.IsValid()) return collection;
                objectCollections.Remove(path);
            }

            UpdateCollections(root);

            collection = new PropertyCollection(root, path);
            objectCollections.Add(path, collection);

            return collection;
        }

        public static bool TryApply(SerializedObject serializedObject)
        {
            if (!serializedObject.hasModifiedProperties) return false;

            serializedObject.ApplyModifiedProperties();
            ResetCollections(serializedObject);

            return true;
        }

        private static void UpdateCollections(SerializedObject serializedObject)
        {
            serializedObject.Update();
            ResetCollections(serializedObject);
        }

        private static void ResetCollections(SerializedObject serializedObject)
        {
            if (!collections.TryGetValue(GetID(serializedObject), out var objectCollections)) return;

            foreach (var (key, collection) in objectCollections.ToList())
            {
                if (collection.IsValid()) collection.Reset();
                else objectCollections.Remove(key);
            }
        }

        #endregion

        private readonly SerializedObject root;
        private readonly Object owner;
        private readonly string propertyPath;

        private readonly Dictionary<string, SerializedProperty> properties = new();

        #region Layout Draw

        public SerializedProperty GetRelative(string propertyPath)
        {
            var paths = propertyPath.Split('.');
            if (!properties.TryGetValue(paths[0], out var pathProperty)) return null;

            for (int i = 1; i < paths.Length; i++)
                pathProperty = pathProperty.FindPropertyRelative(paths[i]);

            return pathProperty;

        }

        public void DrawRelative(string propertyPath)
        {
            var paths = propertyPath.Split('.');
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

        public void DrawList(string propertyName)
        {
            if (!TryGet(propertyName, out var property)) return;

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

        public bool DrawGetBool(string propertyName)
        {
            if (!TryGet(propertyName, out SerializedProperty property)) return false;

            EditorGUILayout.PropertyField(property);
            return property.boolValue;
        }

        public string DrawGetString(string propertyName)
        {
            if (!TryGet(propertyName, out SerializedProperty property)) return null;

            EditorGUILayout.PropertyField(property);
            return property.stringValue;
        }

        public void DrawAll(params string[] including)
        {
            foreach (var entry in including) Draw(entry);
        }

        public void DrawAllExcept(params string[] except)
        {
            foreach (var (key, prop) in properties)
            {
                if (except.Contains(key)) continue;
                Draw(key);
            }
        }

        #endregion

        #region Draw Rect

        public void Draw(ref Rect rect, string propertyName, bool slideRect = false)
        {
            if (TryGet(propertyName, out var property))
                EditorGUI.PropertyField(rect, property);

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawNoLabel(ref Rect rect, string propertyName, bool slideRect = true)
        {
            if (TryGet(propertyName, out var property))
                EditorGUI.PropertyField(rect, property, GUIContent.none);

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawEnum<T>(ref Rect rect, string propertyName, EConsoleColor color = EConsoleColor.Primary, string hint = null, bool slideRect = true) where T: Enum
        {
            if (TryGet(propertyName, out var property))
                EnumTreeGUI.DrawEnum<T>(rect, property, Colors.Console.Get(color), hint);

            if (slideRect) rect.SlideSameRect();
        }

        public void DrawSwitch<T>(ref Rect rect, string propertyName, string hint = null, bool slideRect = true) where T: Enum
        {
            if (TryGet(propertyName, out var property))
                EnumTreeGUI.DrawSwitch<T>(rect, property, hint);

            if (slideRect) rect.SlideSameRect();
        }

        public bool DrawToggle(ref Rect rect, string propertyName, string label = null, string hint = null, bool slideRect = true)
        {
            if (TryGet(propertyName, out var property))
                SkyxGUI.DrawSuccessToggle(rect, string.IsNullOrEmpty(label) ? property.PrettyName() : label, property, hint);

            if (slideRect) rect.SlideSameRect();

            return property?.boolValue ?? false;
        }

        public void DrawList(Rect rect, string propertyName)
        {
            if (!TryGet(propertyName, out var property)) return;

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

        public void RegisterList(string propertyName, bool displayHeader = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        {
            if (!TryGet(propertyName, out var property)) return;
            if (HasList(property)) return;

            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback
            };

            lists.Add(property, list);

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
        }

        public void RegisterList(string propertyName, ReorderableList list)
        {
            if (!TryGet(propertyName, out var property)) return;
            lists.TryAdd(property, list);
        }

        public bool HasList(string propertyName) => TryGet(propertyName, out var property) && lists.ContainsKey(property);
        public bool HasList(SerializedProperty property) => lists.ContainsKey(property);

        #endregion

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

        public bool TryGet(string propertyName, bool isBacking, out SerializedProperty property)
        {
            return isBacking ? TryGetBacking(propertyName, out property) : TryGet(propertyName, out property);
        }

        public bool TryGet(string propertyName, out SerializedProperty property)
        {
            if (properties.TryGetValue(propertyName, out property)) return true;

            Debug.LogError($"{owner} does not contain {propertyName}", owner);
            return false;
        }

        public bool TryGetBacking(string propertyName, out SerializedProperty property)
        {
            propertyName = $"<{propertyName}>k__BackingField";
            if (properties.TryGetValue(propertyName, out property)) return true;

            Debug.LogError($"{owner} does not contain {propertyName}", owner);
            return false;
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

        private void Reset()
        {
            properties.Clear();
            lists.Clear();

            Setup();
        }

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

        private PropertyCollection(SerializedObject root, string path)
        {
            this.root = root;

            owner = root.targetObject;
            propertyPath = path;

            Setup();
        }
    }
}