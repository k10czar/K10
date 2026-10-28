using System.Collections.Generic;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.Explorer
{
    public class ExplorerInspectorInfo
    {
        public readonly IExplorerWindow Window;

        public readonly Object Target;
        public readonly SerializedObject SerializedObject;
        public readonly SerializedProperty Property;
        public readonly Editor ObjectEditor;
        public readonly string PropertyPath;
        public bool IsPinned;

        public readonly List<Object> Breadcrumbs;

        public ExplorerInspectorInfo(IExplorerWindow window, Object target, string propertyPath, List<Object> breadcrumbs, bool isPinned)
        {
            Window = window;
            Target = target;
            PropertyPath = propertyPath;
            Breadcrumbs = breadcrumbs ?? (string.IsNullOrEmpty(propertyPath) ? null : new List<Object> { target });
            IsPinned = isPinned;

            if (string.IsNullOrEmpty(propertyPath))
                ObjectEditor = Editor.CreateEditor(target);
            else
            {
                SerializedObject = PropertyCollection.GetSerializedObject(target);
                Property = SerializedObject.FindProperty(propertyPath);
            }
        }
    }
}