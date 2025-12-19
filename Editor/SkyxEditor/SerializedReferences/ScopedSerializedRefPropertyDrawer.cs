using K10.DebugSystem;
using UnityEditor;
using UnityEngine;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(ScopedSerializedRefAttribute))]
    public class ScopedSerializedRefPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var labelText = property.displayName;
            if (SerializedRefLib.TryDrawMissingRef(ref rect, property, labelText))
                return;

            var alwaysShowChangeRef = true;
            Rect? buttonRect = null;

            if (alwaysShowChangeRef)
            {
                bool clicked;
                (buttonRect, clicked) = RectLib.ExtractOverHeaderButton(rect, EElementSize.SingleLine);

                if (clicked)
                {
                    SerializedRefLib.DrawTypePickerMenu(buttonRect.Value, property);
                    return;
                }
            }

            var currentValue = property.GetValue();
            var summarizable = currentValue as IContentSummary;
            var isSummarizable = summarizable != null;

            var info = isSummarizable ? summarizable.Summary : property.managedReferenceValue?.GetType().Name;
            var color = isSummarizable ? summarizable.SummaryColor : EColor.Secondary;

            labelText = labelText.AppendInfo(info, EColor.Support, EElementSize.SingleLine);

            if (!property.hasVisibleChildren)
            {
                FoldoutScope.HeaderOnly(ref rect, labelText);
                if (alwaysShowChangeRef) SkyxGUI.Button(buttonRect.Value, "⚙️", EColor.Special, EElementSize.Mini);
                return;
            }

            using var scope = FoldoutScope.Open(ref rect, property, labelText, color, indent: true);
            if (alwaysShowChangeRef) SkyxGUI.Button(buttonRect.Value, "⚙️", EColor.Special, EElementSize.Mini);
            if (!scope.IsExpanded) return;

            if (isSummarizable && summarizable.Description != null)
            {
                rect.ExtractLineDef(out var startX, out var totalWidth);
                EditorGUI.LabelField(rect, summarizable.Description, SkyxStyles.DefaultLabel);

                rect.NextLine(startX, totalWidth);
                SkyxGUI.Separator(ref rect);
            }

            property.DrawAllInnerProperties(ref rect, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null || !property.hasVisibleChildren) return SkyxStyles.FullLineHeight;
            if (!property.isExpanded) return SkyxStyles.ClosedScopeHeight(EElementSize.SingleLine);

            var height = SkyxStyles.ScopeTotalExtraHeight(EElementSize.SingleLine);
            height += property.GetPropertyHeight(true);

            var currentValue = property.GetValue();

            if (currentValue is IContentSummary summarizable && summarizable.Description != null)
                height += SkyxStyles.FullLineHeight + 6;

            return height;
        }
    }
}