using System;
using Rogue.REditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.Explorer
{
    [CustomPropertyDrawer(typeof(ExplorerFilter), true)]
    public class ExplorerFilterPropertyDrawer : PropertyEditor
    {
        protected override void Draw(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect.ExtractLineDef(out var startX, out var totalWidth);
            rect.AdjustToLine(false);

            if (SerializedRefLib.TryDrawMissingRef(ref rect, property, label.text))
                return;

            var target = GetTarget<ExplorerFilter>(property);
            if (target == null) return;

            var invertProperty = property.FindPropertyRelative("invert");
            var valueProperty = property.FindPropertyRelative("value");
            var mustHaveAllProperty = property.FindPropertyRelative("mustHaveAll");

            var hasSecondary = target.SecondaryDrawInfo != null;

            SkyxGUI.MiniWarningToggle(ref rect, invertProperty, "!", "", "Invert filter?");

            if (mustHaveAllProperty != null)
                SkyxGUI.MiniChoiceToggle(ref rect, mustHaveAllProperty, "∪", "∩", "Must have All(∪) or Any(∩)?");

            SkyxGUI.DrawLabel(ref rect, target.Name, !hasSecondary);

            if (hasSecondary) rect.NextDividedLine(startX, totalWidth, 2);

            if (target.DrawInfo != null)
                SkyxGUI.Draw(rect, valueProperty, target.DrawInfo.requestedType, target.DrawInfo);

            if (hasSecondary)
            {
                rect.SlideSame();
                var value2Property = property.FindPropertyRelative("value2");
                SkyxGUI.Draw(rect, value2Property, target.SecondaryDrawInfo.requestedType, target.SecondaryDrawInfo);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;
            var value2Property = property.FindPropertyRelative("value2");
            return (value2Property != null ? 2 : 1) * SkyxStyles.FullLineHeight;
        }
    }

    [CustomPropertyDrawer(typeof(TypeSourcedFilter), true)]
    public class TypeFilterPropertyDrawer : PropertyEditor
    {
        protected override void Draw(Rect rect, SerializedProperty property, GUIContent label)
        {
            var target = GetTarget<TypeSourcedFilter>(property);
            if (target == null) return;

            var invertProperty = property.FindPropertyRelative("invert");
            var valueProperty = property.FindPropertyRelative("value");

            rect.AdjustToLine();

            SkyxGUI.MiniWarningToggle(ref rect, invertProperty, "!", "", "Invert filter?");

            SkyxGUI.DrawLabel(ref rect, target.Name);

            var currentType = Type.GetType(valueProperty.stringValue);
            if (EditorGUI.DropdownButton(rect, new GUIContent(currentType?.Name ?? "NULL"), FocusType.Passive))
                ClassTreePicker.Draw(rect, target.TargetType, currentType, type => OnTypeSelected(valueProperty, type));
        }

        private static void OnTypeSelected(SerializedProperty valueProp, Type selectedType)
        {
            valueProp.stringValue = selectedType.AssemblyQualifiedName;
            valueProp.Apply();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.FullLineHeight;
    }
}