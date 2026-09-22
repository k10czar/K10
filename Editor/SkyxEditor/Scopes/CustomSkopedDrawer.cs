using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public abstract class CustomSkopedDrawer : SkopedDrawer
    {
        private ScopedAttribute _scopedAttribute;

        protected virtual EScopePreset ScopePreset => EScopePreset.FoldoutNameSummary;

        private void InitializeAttribute()
        {
            if (_scopedAttribute != null) return;
            _scopedAttribute = new ScopedAttribute(ScopePreset);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            InitializeAttribute();
            OnGUI(rect, property, _scopedAttribute);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitializeAttribute();
            return GetPropertyHeight(property, _scopedAttribute);
        }
    }
}