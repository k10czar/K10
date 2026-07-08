using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(OptionalStringAttribute))]
    public class OptionalStringAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var optionalAtt = (OptionalStringAttribute) attribute;

            if (optionalAtt.showLabel)
                EditorGUI.LabelField(rect.ExtractLabelRect(), label);

            var isSetToOptional = property.stringValue == optionalAtt.optionalValue;

            if (isSetToOptional)
            {
                if (SkyxGUI.Button(rect, optionalAtt.compact))
                {
                    property.stringValue = optionalAtt.defaultValue;
                    property.Apply();
                }
            }
            else
            {
                if (SkyxGUI.MiniButton(ref rect, "!", EColor.Support, optionalAtt.hint, true))
                {
                    property.stringValue = optionalAtt.optionalValue;
                    property.Apply();
                }

                EditorGUI.PropertyField(rect, property, GUIContent.none);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}