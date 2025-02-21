using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(EnumEntry))]
    public class EnumEntryPropertyDrawer : PropertyDrawer
    {
        public static string GetFieldName(FieldInfo[] fields, Type target) => fields.First(entry => entry.FieldType == target).Name;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var entry = (EnumEntry) attribute;

            var hasTarget = entry.enumType != null;

            var targetType = hasTarget ? entry.enumType : property.GetValue().GetType();
            var targetProperty = hasTarget ? property.FindPropertyRelative(GetFieldName(targetType.GetFields(), entry.enumType)) : property;

            EnumTreeGUI.DrawEnum(rect, targetProperty, targetType, Colors.Console.Get(entry.color));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }

    [CustomPropertyDrawer(typeof(EnumAndFieldEntry))]
    public class EnumAndFieldEntryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var entry = (EnumAndFieldEntry) attribute;

            var obj = property.GetValue();
            var fields = obj.GetType().GetFields();
            var firstField = EnumEntryPropertyDrawer.GetFieldName(fields, entry.firstType);
            var secondField = EnumEntryPropertyDrawer.GetFieldName(fields, entry.secondType);

            rect.AdjustToLineAndDivide(2);
            EnumTreeGUI.DrawEnum(rect, property.FindPropertyRelative(firstField), entry.firstType, Colors.Console.Get(entry.firstColor), entry.firstHint);
            rect.SlideSameRect();

            if (entry.secondType.IsEnum)
                EnumTreeGUI.DrawEnum(rect, property.FindPropertyRelative(secondField), entry.secondType, Colors.Console.Get(entry.secondColor), entry.secondHint);

            else if (entry.secondType.IsClass)
                SkyxGUI.DrawObjectField(rect, property.FindPropertyRelative(secondField), entry.secondType, entry.secondHint, true);

            else if (entry.secondType == typeof(float))
                SkyxGUI.DrawFloatField(rect, property.FindPropertyRelative(secondField), entry.secondHint);

            else if (entry.secondType == typeof(int))
                SkyxGUI.DrawIntField(rect, property.FindPropertyRelative(secondField), entry.secondHint);

            else if (entry.secondType == typeof(string))
                SkyxGUI.DrawTextField(rect, property.FindPropertyRelative(secondField), entry.secondHint);

            else throw new Exception("Unknown type");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }

    [CustomPropertyDrawer(typeof(EnumAndMaskEntry))]
    public class EnumAndMaskEntryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var entry = (EnumAndMaskEntry) attribute;

            var obj = property.GetValue();
            var fields = obj.GetType().GetFields();
            var firstField = EnumEntryPropertyDrawer.GetFieldName(fields, entry.firstType);
            var secondField = EnumEntryPropertyDrawer.GetFieldName(fields, typeof(int));

            rect.AdjustToLineAndDivide(2);
            EnumTreeGUI.DrawEnum(rect, property.FindPropertyRelative(firstField), entry.firstType, Colors.Console.Get(entry.firstColor));
            rect.SlideSameRect();

            EnumTreeGUI.DrawEnumMask(rect, property.FindPropertyRelative(secondField), entry.secondType, entry.secondColor);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}