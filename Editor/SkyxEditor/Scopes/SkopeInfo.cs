using System;
using Skyx.RuntimeEditor;
using UnityEditor;

namespace Skyx.SkyxEditor
{
    public class SkopeInfo
    {
        public readonly EScopeType scopeType;
        public readonly EColor color;
        public readonly EElementSize size;
        public readonly string title;
        public readonly SerializedProperty property;
        public readonly bool indent = false;
        public (string, EColor, Action)[] buttons;

        public void SetButtons(params (string, EColor, Action)[] buttonsArray) => buttons = buttonsArray;

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color, EElementSize size)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = title;
            this.color = color;
            this.size = size;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color, EElementSize size)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = property.displayName;
            this.color = color;
            this.size = size;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, string title, EColor color, EElementSize size, bool indent)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = title;
            this.color = color;
            this.size = size;
            this.indent = indent;
        }

        public SkopeInfo(EScopeType scopeType, SerializedProperty property, EColor color, EElementSize size, bool indent)
        {
            this.scopeType = scopeType;
            this.property = property;
            this.title = property.displayName;
            this.color = color;
            this.size = size;
            this.indent = indent;
        }
    }

    public static class SkopeInfoExtensions
    {
        public static SkopeInfo GetSkope(this ScopedAttribute attribute, SerializedProperty property, string title, EColor color)
            => new(attribute.scopeType, property, title, color, attribute.elementSize, attribute.indent);
    }
}