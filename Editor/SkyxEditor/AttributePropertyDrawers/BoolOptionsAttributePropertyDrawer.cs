using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(BoolOptionsAttribute))]
    public class BoolOptionsAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var optionsAtt = (BoolOptionsAttribute) attribute;
            var (text, color) = optionsAtt.Get(property.boolValue);

            EditorGUI.LabelField(rect.ExtractLabelRect(), optionsAtt.GetLabel(label.text));

            if (SkyxGUI.Button(rect, text, color))
            {
                property.boolValue = !property.boolValue;
                property.Apply();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}