using K10.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(InlineSerializedRefAttribute))]
    public class InlineSerializedRefPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var outerRect = rect;
            outerRect.x -= SkyxStyles.BoxMargin - 1;
            outerRect.width += (SkyxStyles.BoxMargin - 1) * 2;
            EditorGUI.DrawRect(outerRect, Colors.Console.BackgroundDarkFilter);

            var labelText = property.displayName;
            rect.ExtractLineDef(out var startX, out var totalWidth);
            rect.AdjustToLine();

            SkyxGUI.DrawLabel(ref rect, labelText);

            if (property.managedReferenceValue == null)
            {
                if (SkyxGUI.Button(rect, "MISSING REFERENCE!", EColor.Danger))
                    SerializedRefLib.DrawTypePickerMenu(rect, property);

                return;
            }

            var currentValue = property.GetValue();
            var summarizable = currentValue as IContentSummary;
            var isSummarizable = summarizable != null;

            var color = isSummarizable ? summarizable.SummaryColor : EColor.Secondary;

            if (SkyxGUI.Button(rect, property.managedReferenceValue?.GetType().Name.Pretty(), color))
                SerializedRefLib.DrawTypePickerMenu(rect, property);

            rect.NextLine(startX, totalWidth);

            if (isSummarizable && summarizable.Description != null)
            {
                EditorGUI.LabelField(rect, summarizable.Description, SkyxStyles.DefaultLabel);

                rect.NextLine(startX, totalWidth);
                SkyxGUI.Separator(ref rect);
            }

            EditorGUI.indentLevel++;
            property.DrawAllInnerProperties(ref rect, true);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null) return SkyxStyles.FullLineHeight;
            if (!property.isExpanded) return SkyxStyles.ClosedScopeHeight(EElementSize.SingleLine);

            var height = SkyxStyles.FullLineHeight + property.GetPropertyHeight(true);

            var currentValue = property.GetValue();
            if (currentValue is IContentSummary summarizable && summarizable.Description != null)
                height += SkyxStyles.FullLineHeight + 6;

            return height;
        }
    }
}