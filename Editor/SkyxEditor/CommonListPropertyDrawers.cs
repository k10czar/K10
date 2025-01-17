using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(SingleEnumEntry))]
    public class SingleEnumEntryPropertyDrawer : PropertyDrawer
    {
        public static string GetFieldName(FieldInfo[] fields, Type target) => fields.First(entry => entry.FieldType == target).Name;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var entry = (SingleEnumEntry) attribute;

            var hasTarget = entry.enumType != null;

            var targetType = hasTarget ? entry.enumType : property.GetValue().GetType();
            var targetProperty = hasTarget ? property.FindPropertyRelative(GetFieldName(targetType.GetFields(), entry.enumType)) : property;

            EnumTreeGUI.DrawEnum(rect, targetProperty, targetType, Colors.Console.Get(entry.color));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }

    [CustomPropertyDrawer(typeof(DoubleEnumEntry))]
    public class DoubleEnumEntryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var entry = (DoubleEnumEntry) attribute;

            var obj = property.GetValue();
            var fields = obj.GetType().GetFields();
            var firstField = SingleEnumEntryPropertyDrawer.GetFieldName(fields, entry.firstType);
            var secondField = SingleEnumEntryPropertyDrawer.GetFieldName(fields, entry.secondType);

            rect.AdjustToLineAndDivide(2);
            EnumTreeGUI.DrawEnum(rect, property.FindPropertyRelative(firstField), entry.firstType, Colors.Console.Get(entry.firstColor));
            rect.SlideSameRect();

            if (entry.secondType.IsEnum)
                EnumTreeGUI.DrawEnum(rect, property.FindPropertyRelative(secondField), entry.secondType, Colors.Console.Get(entry.secondColor));

            else if (entry.secondType.IsClass)

                SkyxGUI.DrawObjectField(rect, property.FindPropertyRelative(secondField), entry.secondType, null, true);

            else throw new Exception("Unknown type");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}