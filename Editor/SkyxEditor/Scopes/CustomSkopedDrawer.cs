using Skyx.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    public abstract class CustomSkopedDrawer : SkopedDrawer
    {
        private ScopedAttribute scopedAttribute;

        protected virtual EScopePreset ScopePreset => EScopePreset.FoldoutNameSummary;

        private void InitializeAttribute()
        {
            if (scopedAttribute != null) return;
            scopedAttribute = new ScopedAttribute(ScopePreset);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            InitializeAttribute();
            OnGUI(rect, property, scopedAttribute);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitializeAttribute();
            return GetPropertyHeight(property, scopedAttribute);
        }
    }
}