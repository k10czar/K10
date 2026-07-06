using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rogue.REditor
{
    public static class ReorderableListCache
    {
        #region Cache Interface

        private static readonly Dictionary<(int, string), ReorderableList> cache = new();

        public static void Add(SerializedProperty property, ReorderableList list) => Add(property.GetCacheID(), list);
        public static void Add((int, string) cacheID, ReorderableList list) => cache[cacheID] = list;

        public static bool HasList(SerializedProperty property)
        {
            var cacheID = property.GetCacheID();
            return cache.ContainsKey(cacheID);
        }

        public static ReorderableList Get(SerializedProperty property)
        {
            var cacheID = property.GetCacheID();
            if (cache.TryGetValue(cacheID, out var list)) return list;

            Debug.LogError($"ReorderableList for {property} not found!");
            return null;
        }

        public static bool TryGet(SerializedProperty property, out ReorderableList list)
        {
            if (!TryGet(property.GetCacheID(), out list)) return false;
            return list.serializedProperty == property;
        }

        public static bool TryGet((int, string) cacheID, out ReorderableList list) => cache.TryGetValue(cacheID, out list);

        public static ReorderableList GetOrCreate(SerializedProperty property, Func<SerializedProperty, ReorderableList> create)
        {
            var cacheID = property.GetCacheID();
            if (cache.TryGetValue(cacheID, out var list)) return list;

            list = create(property);
            list.FixFooterHeight();

            cache.Add(cacheID, list);

            return list;
        }

        public static ReorderableList GetOrCreate((int, string) cacheID, Func<ReorderableList> create)
        {
            if (cache.TryGetValue(cacheID, out var list)) return list;

            list = create();
            list.FixFooterHeight();

            cache.Add(cacheID, list);

            return list;
        }

        public static void InvalidateCacheFromTarget(int mainCacheID)
        {
            var keysToRemove = cache.Keys.Where(k => k.Item1 == mainCacheID).ToList();

            foreach (var key in keysToRemove)
                cache.Remove(key);
        }

        public static void Clear() => cache.Clear();

        static ReorderableListCache() => Selection.selectionChanged += Clear;

        #endregion

        #region Default List Creation

        #region List Draw Delegates

        public delegate bool IsElementHighlighted(SerializedProperty elementProperty);
        public delegate void DrawElement(SerializedProperty property, Rect rect, int index, bool isActive, bool isFocused);
        public delegate void DrawListHeader(SerializedProperty property, Rect rect);

        #endregion

        public static ReorderableList CreateDefaultList(
            SerializedProperty property,
            bool displayHeader = true,
            bool draggable = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true,
            DrawElement customDrawElement = null,
            Action<SerializedProperty> newElementSetup = null,
            DrawListHeader customHeader = null,
            IsElementHighlighted isElementHighlighted = null)
        {
            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback,
                onAddCallback = OnAddCallback,
                onRemoveCallback = OnRemoveCallback,
                onReorderCallback = OnReorderCallback,
                drawElementBackgroundCallback = DrawElementBackgroundCallback,
            };

            list.FixFooterHeight();

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
                else customDrawElement(property, rect, index, isActive, isFocused);
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
                else customHeader(property, rect);
            }

            void OnAddCallback(ReorderableList thisList)
            {
                var index = thisList.selectedIndices.Count > 0 ? Mathf.Min(property.arraySize, thisList.selectedIndices[0] + 1) : property.arraySize;
                property.InsertArrayElementAtIndex(index);
                property.Apply($"New array element: {property.propertyPath}");

                var newElement = property.GetArrayElementAtIndex(index);
                newElement.ResetDefaultValues(newElementSetup, false, true);
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

        public static void FixFooterHeight(this ReorderableList list)
        {
            if (!list.displayAdd && !list.displayRemove)
                list.footerHeight = 0;
        }

        #endregion
    }
}