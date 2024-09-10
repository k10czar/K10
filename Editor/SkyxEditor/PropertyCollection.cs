using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    public class PropertyCollection : Dictionary<string, SerializedProperty>
    {
        #region Static

        [ResetedOnLoad] private static readonly Dictionary<string, PropertyCollection> collections = new();

        private static string GetID(SerializedObject target) => target.targetObject.GetHashCode().ToString();
        private static string GetID(SerializedProperty target) => $"{target.serializedObject.targetObject.GetHashCode()}|{target.propertyPath}";

        public static PropertyCollection Get(SerializedObject serializedObject, bool forceReset = false)
            => Get(GetID(serializedObject), serializedObject.GetIterator(), true, forceReset);

        public static PropertyCollection Get(SerializedProperty serializedProperty, bool forceReset = false)
            => Get(GetID(serializedProperty), serializedProperty, false, forceReset);

        private static PropertyCollection Get(string id, SerializedProperty serializedProperty, bool fromObject, bool forceReset)
        {
            if (collections.TryGetValue(id, out var collection))
            {
                if (!forceReset && collection.IsValid()) return collection;
                collections.Remove(id);
            }

            collection = new PropertyCollection(serializedProperty.serializedObject.targetObject);
            collections.Add(id, collection);

            var currentProperty = serializedProperty.Copy();
            if (!currentProperty.NextVisible(true)) return collection;

            var nextSiblingProperty = serializedProperty.Copy();
            if (!fromObject) nextSiblingProperty.NextVisible(false);

            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty)) break;

                collection.Add(currentProperty.name, currentProperty.Copy());
            }
            while (currentProperty.NextVisible(false));

            return collection;
        }

        #endregion

        private readonly Object owner;
        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

        #region Layout Draw

        public SerializedProperty GetRelative(string propertyPath)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                return pathProperty;
            }

            return null;
        }

        public void DrawRelative(string propertyPath)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                EditorGUILayout.PropertyField(pathProperty);
            }
        }

        public void DrawRelative(string propertyPath, GUIContent label)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                EditorGUILayout.PropertyField(pathProperty, label);
            }
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
            propertyName = $"<{propertyName}>k__BackingField";
            if (TryGet(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property);
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

        public void DrawAll(bool indentDropdowns = false, params string[] except)
        {
            foreach (var property in this)
            {
                if (except.Contains(property.Key)) continue;

                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public void DrawAllPredicate(bool indentDropdowns, int skip, Predicate<string> predicate)
        {
            foreach (var property in this.Skip(skip))
            {
                if (!predicate(property.Key))
                    continue;

                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public void Draw(string propertyName, bool includeChildren)
        {
            if (TryGet(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        public void Draw(string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, label);
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

        public void DrawEnum<T>(Rect rect, string propertyName, Colors.EConsoleColor color = Colors.EConsoleColor.Primary) where T: Enum
        {
            if (TryGet(propertyName, out var property))
                EnumTreeGUI.DrawEnum(rect, property, typeof(T), Colors.Console.Get(color));
        }

        #endregion

        public float GetHeight(params string[] excludeFields)
        {
            var total = 0f;

            foreach (var (field, property) in this)
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

        private bool TryGet(string propertyName, out SerializedProperty property)
        {
            if (TryGetValue(propertyName, out property)) return true;

            Debug.LogError($"{owner} does not contain {propertyName}", owner);
            return false;
        }

        private bool IsValid()
        {
            try
            {
                _ = Values.All(entry => entry.isExpanded); // Forces internal Verify call
                return true;
            }
            catch
            {
                return false;
            }
        }

        private PropertyCollection(Object owner)
        {
            this.owner = owner;
        }
    }
}