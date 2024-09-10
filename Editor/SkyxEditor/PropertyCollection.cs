using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditorInternal;

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

            Debug.Log($"Creating property collection! {id} {serializedProperty} | {forceReset}");

            collection = new PropertyCollection();
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

        public static PropertyCollection GetAllProperties(SerializedObject serializedObject)
        {
            Debug.Log($"GetAllProperties! {serializedObject}");
            PropertyCollection properties = new();

            SerializedProperty property = serializedObject.GetIterator();
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty nextSiblingProperty = property.Copy();

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    properties.Add(currentProperty.name, currentProperty.Copy());
                }
                while (currentProperty.NextVisible(false));
            }

            return properties;
        }

        #endregion

        private readonly Dictionary<SerializedProperty, ReorderableList> lists = new();

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
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                if (indent > 0) EditorGUI.indentLevel += indent;
                EditorGUILayout.PropertyField(property);
                if (indent > 0) EditorGUI.indentLevel -= indent;
            }
        }

        public void DrawList(string propertyName, bool displayHeader = false, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        {
            if (!TryGetValue(propertyName, out var property)) return;

            if (!lists.TryGetValue(property, out var list))
            {
                list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
                {
                    drawElementCallback = DrawElementCallback,
                    // elementHeightCallback = ElementHeightCallback
                };

                // float ElementHeightCallback(int index)
                // {
                //     var target = property.GetArrayElementAtIndex(index);
                //     return target.GetPropertyChildHeight()
                // }

                void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
                {
                    var target = property.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, target);
                }

                lists.Add(property, list);
            }

            list.DoLayoutList();
        }

        public void DrawBacking(string propertyName)
        {
            propertyName = $"<{propertyName}>k__BackingField";
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property);
        }

        public bool DrawGetBool(string propertyName)
        {
            if (!TryGetValue(propertyName, out SerializedProperty property)) return false;

            EditorGUILayout.PropertyField(property);
            return property.boolValue;
        }

        public string DrawGetString(string propertyName)
        {
            if (!TryGetValue(propertyName, out SerializedProperty property)) return null;

            EditorGUILayout.PropertyField(property);
            return property.stringValue;
        }

        public bool DrawToggleLeft(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                GUIContent label = new(property.displayName, property.tooltip);
                return property.boolValue = EditorGUILayout.ToggleLeft(label, property.boolValue);
            }

            return false;
        }

        public void DrawAll(bool indentDropdowns = false, int skip = 0)
        {
            foreach (var property in this.Skip(skip))
            {
                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public void DrawAllExcept(bool indentDropdowns = false, int skip = 0, params string[] except)
        {
            foreach (var property in this.Skip(skip))
            {
                if (except.Contains(property.Key))
                    continue;

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

        public bool BoolValue(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                return property.boolValue;

            return false;
        }

        public void DrawArray(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                if (property.isArray) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property);
                if (property.isArray) EditorGUI.indentLevel--;
            }
        }

        public void Draw(string propertyName, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        public void Draw(string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, label);
        }

        public bool DrawGetBool(string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                EditorGUILayout.PropertyField(property, label);
                return property.boolValue;
            }

            return false;
        }

        public void Draw(string propertyName, GUIContent label, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, label, includeChildren);
        }

        public void Draw(Rect rect, string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property);
        }

        public void Draw(Rect rect, string propertyName, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, includeChildren);
        }

        public void Draw(Rect rect, string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, label);
        }

        public void Draw(Rect rect, string propertyName, GUIContent label, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, label, includeChildren);
        }
    }
}