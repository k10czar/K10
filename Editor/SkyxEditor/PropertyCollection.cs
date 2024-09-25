﻿using System.Collections.Generic;
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
        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

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

        public void DrawList(string propertyName, bool displayHeader = false, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        {
            if (!TryGet(propertyName, out var property)) return;

            var list = GetReorderableList(property, displayHeader, draggable, displayAddButton, displayRemoveButton);
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

        public void DrawAll(params string[] properties)
        {
            foreach (var entry in properties) Draw(entry);
        }

        #endregion

        #region Draw Rect

        public void Draw(Rect rect, string propertyName)
        {
            if (TryGet(propertyName, out var property))
                EditorGUI.PropertyField(rect, property);
        }

        public void DrawNoLabel(Rect rect, string propertyName)
        {
            if (TryGet(propertyName, out var property))
                EditorGUI.PropertyField(rect, property, GUIContent.none);
        }

        public string DrawGetString(Rect rect, string propertyName)
        {
            if (!TryGet(propertyName, out var property)) return null;

            EditorGUI.PropertyField(rect, property);
            return property.stringValue;
        }

        public void DrawList(Rect rect, string propertyName, bool displayHeader = false, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        {
            if (!TryGet(propertyName, out var property)) return;

            var list = GetReorderableList(property, displayHeader, draggable, displayAddButton, displayRemoveButton);
            list.DoList(rect);
        }

        public void DrawEnum<T>(Rect rect, string propertyName, Colors.EConsoleColor color = Colors.EConsoleColor.Primary, string hint = null) where T: Enum
        {
            if (TryGet(propertyName, out var property))
                EnumTreeGUI.DrawEnum(rect, property, typeof(T), Colors.Console.Get(color), hint);
        }

        #endregion

        #region Scopes

        public FoldoutBoxScope GetFoldoutScope(string propertyName, bool isBacking, string name = null)
        {
            return TryGet(propertyName, isBacking, out var property)
                ? new FoldoutBoxScope(property, string.IsNullOrEmpty(name) ? property.PrettyName() : name)
                : null;
        }


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

        private ReorderableList GetReorderableList(SerializedProperty property, bool displayHeader = false, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        {
            if (lists.TryGetValue(property, out var list))
            {
                Debug.Assert(list.draggable == draggable && list.displayAdd == displayAddButton && list.displayRemove == displayRemoveButton && ((displayHeader && list.drawHeaderCallback != null) || (!displayHeader && list.drawHeaderCallback == null)), "Reorderable list with incompatible configs");
                return list;
            }

            list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback
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