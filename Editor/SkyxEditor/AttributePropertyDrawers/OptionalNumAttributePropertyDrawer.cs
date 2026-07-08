using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(OptionalNumAttribute))]
    public class OptionalNumAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var optionalAtt = (OptionalNumAttribute) attribute;

            if (optionalAtt.showLabel)
                EditorGUI.LabelField(rect.ExtractLabelRect(), label);

            var isFloat = property.propertyType is SerializedPropertyType.Float;
            var isSetToOptional = isFloat
                ? (optionalAtt.useInfinite ? float.IsInfinity(property.floatValue) : property.floatValue < 0)
                : (optionalAtt.useInfinite ? property.intValue == int.MaxValue : property.intValue < 0);

            if (isSetToOptional)
            {
                if (SkyxGUI.Button(rect, optionalAtt.compact))
                {
                    if (isFloat) property.floatValue = 1;
                    else property.intValue = 1;
                    property.Apply();
                }
            }
            else
            {
                if (SkyxGUI.MiniButton(ref rect, "!", EColor.Support, optionalAtt.hint, true))
                {
                    if (isFloat) property.floatValue = optionalAtt.useInfinite ? float.MaxValue : -1;
                    else property.intValue = optionalAtt.useInfinite ? int.MaxValue : -1;
                    property.Apply();
                }

                EditorGUI.PropertyField(rect, property, GUIContent.none);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}