using UnityEditor;
using UnityEngine;

namespace K10.EditorUtils
{
    public static class GenericMenuExtensions
    {
        public static void AddItem(this GenericMenu menu, string label, GenericMenu.MenuFunction action)
            => menu.AddItem(new GUIContent(label), false, action);
    }
}