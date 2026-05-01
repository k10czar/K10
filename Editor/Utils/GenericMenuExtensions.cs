using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace K10.EditorUtils
{
    public static class GenericMenuExtensions
    {
        public static void AddItem(this GenericMenu menu, string label, GenericMenu.MenuFunction action)
            => menu.AddItem(new GUIContent(label), false, action);

        public static void AddItem(this GenericDropdownMenu menu, string label, Action action)
            => menu.AddItem(label, false, action);
    }
}