using System;
using UnityEditor;

namespace Skyx.RuntimeEditor
{
    public class EditorButtonDef<T>
    {
        public readonly string label;
        public readonly string tooltip;
        public readonly Action<T> onClick;
        public EColor color;
        public bool isDisabled;

        protected EditorButtonDef(string label, EColor color, Action<T> onClick, string tooltip = null)
        {
            this.label = label;
            this.color = color;
            this.onClick = onClick;
            this.tooltip = tooltip;
        }

        public override bool Equals(object obj) => obj is EditorButtonDef<T> other && Equals(other);
        private bool Equals(EditorButtonDef<T> other) => label == other.label;
        public override int GetHashCode() => label.GetHashCode();
    }

    #if UNITY_EDITOR
    public class SkopeButton : EditorButtonDef<SerializedProperty>
    {
        public SkopeButton(string label, EColor color, Action<SerializedProperty> onClick, string tooltip = null)
            : base(label, color, onClick, tooltip) {}
    }
    #endif
}