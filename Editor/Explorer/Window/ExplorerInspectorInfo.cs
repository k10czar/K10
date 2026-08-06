using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rogue.Explorer
{
    public class ExplorerInspectorInfo
    {
        public readonly IExplorerWindow window;

        public readonly Object target;
        public readonly SerializedObject serializedObject;
        public readonly SerializedProperty property;
        public readonly Editor objectEditor;
        public readonly string propertyPath;
        public bool isPinned;

        public readonly List<Object> breadcrumbs;

        public ExplorerInspectorInfo(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs, bool isPinned)
        {
            this.window = window;
            this.target = target;
            this.propertyPath = propertyPath;
            this.breadcrumbs = breadcrumbs ?? (string.IsNullOrEmpty(propertyPath) ? null : new List<Object> { target });
            this.isPinned = isPinned;

            if (string.IsNullOrEmpty(propertyPath))
                objectEditor = Editor.CreateEditor(target);
            else
            {
                serializedObject = new SerializedObject(target);
                property = serializedObject.FindProperty(propertyPath);
            }
        }
    }
}