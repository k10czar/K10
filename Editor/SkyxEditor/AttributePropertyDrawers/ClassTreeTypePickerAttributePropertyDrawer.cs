using System;
using Rogue.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(ClassTreeTypePickerAttribute))]
    public class ClassTreeTypePickerAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var classAtt = (ClassTreeTypePickerAttribute) attribute;

            SkyxGUI.DrawLabel(ref rect, label.text);

            var currentType = Type.GetType(property.stringValue);
            if (EditorGUI.DropdownButton(rect, new GUIContent(currentType?.Name ?? "NULL"), FocusType.Passive))
                ClassTreePicker.Draw(rect, classAtt.targetType, currentType, type => OnTypeSelected(property, type));
        }

        private static void OnTypeSelected(SerializedProperty valueProp, Type selectedType)
        {
            valueProp.stringValue = selectedType.AssemblyQualifiedName;
            valueProp.Apply();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}