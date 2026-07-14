using System;
using UnityEditor;
using UnityEngine;

namespace K10.Optimization.Editor
{
    // The three memory (GB) upper bounds that drive HardwareTier: mem <= low => Low, <= mid => Mid,
    // <= high => High, otherwise Extreme. A bound that is not greater than the running maximum simply
    // skips that tier (e.g. { 6, 0, 0 } yields only Low and Extreme).
    [Serializable]
    public class MemoryTierThresholds
    {
        [Tooltip ("Upper bound (GB) for the Low tier.")] public float low = 6f;
        [Tooltip ("Upper bound (GB) for the Mid tier. Set it above Low to enable Mid.")] public float mid = 0f;
        [Tooltip ("Upper bound (GB) for the High tier. Set it above Mid to enable High.")] public float high = 0f;

        public float[] ToArray () => new[] { low, mid, high };
    }

    [CustomPropertyDrawer (typeof (MemoryTierThresholds))]
    public class MemoryTierThresholdsDrawer : PropertyDrawer
    {
        static readonly GUIContent[] FieldLabels =
        {
            new GUIContent ("Low", "Upper bound (GB) for the Low tier."),
            new GUIContent ("Mid", "Upper bound (GB) for the Mid tier."),
            new GUIContent ("High", "Upper bound (GB) for the High tier."),
        };

        const float Spacing = 4f;
        const float ColumnSpacing = 4f;
        const float SubLabelWidth = 30f;
        const float PreviewIndent = 12f;
        const float OneLineFieldsWidth = 500f;
        const float PreviewGap = 12f;
        const float LayoutMargin = 30f; // slack for list handles / scrollbar so narrow rows don't overflow

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            bool oneLine = FitsOneLine (property, out _, out _);
            return oneLine
                ? EditorGUIUtility.singleLineHeight
                : 2 * EditorGUIUtility.singleLineHeight + Spacing;
        }

        public override void OnGUI (Rect area, SerializedProperty property, GUIContent label)
        {
            SerializedProperty low = property.FindPropertyRelative ("low");
            SerializedProperty mid = property.FindPropertyRelative ("mid");
            SerializedProperty high = property.FindPropertyRelative ("high");

            float h = EditorGUIUtility.singleLineHeight;
            bool oneLine = FitsOneLine (property, out string preview, out float previewWidth);

            // First row: prefix label + the three bounds as separate float fields.
            Rect firstRow = new Rect (area.x, area.y, area.width, h);
            Rect fields = EditorGUI.PrefixLabel (firstRow, label);

            Rect fieldsArea, previewRect;
            if (oneLine)
            {
                // Shrink the fields to a fixed block and put the preview to their right, all on one line.
                float fw = Mathf.Max (60f, Mathf.Min (OneLineFieldsWidth, fields.width - previewWidth - PreviewGap));
                float px = fields.x + fw + PreviewGap;
                var pw = Mathf.Min (previewWidth, Mathf.Max (0f, fields.xMax - px));
                var pwSp = pw + Spacing;
                previewRect = new Rect (fields.xMax - pwSp, fields.y, pw, fields.height);
                fieldsArea = new Rect (fields.x, fields.y, fields.width - ( pwSp + PreviewGap ), fields.height);
            }
            else
            {
                // Fields take the full width; the preview drops to a second row.
                fieldsArea = fields;
                float px = area.x + PreviewIndent;
                previewRect = new Rect (px, area.y + h + Spacing, Mathf.Min (previewWidth, area.xMax - px), h);
            }

            DrawBounds (fieldsArea, low, mid, high, h);
            EditorGUI.LabelField (previewRect, preview, EditorStyles.miniLabel);
        }

        static void DrawBounds (Rect area, SerializedProperty low, SerializedProperty mid, SerializedProperty high, float h)
        {
            int prevIndent = EditorGUI.indentLevel;
            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = SubLabelWidth;

            SerializedProperty[] bounds = { low, mid, high };
            float colWidth = (area.width - ColumnSpacing * 2f) / 3f;
            for (int i = 0; i < 3; i++)
            {
                Rect col = new Rect (area.x + i * (colWidth + ColumnSpacing), area.y, colWidth, h);
                EditorGUI.PropertyField (col, bounds[i], FieldLabels[i]);
                EditorGUI.LabelField (col, "GB", K10GuiStyles.unitStyle);
            }

            EditorGUIUtility.labelWidth = prevLabelWidth;
            EditorGUI.indentLevel = prevIndent;
        }

        // Decides whether the fields and the whole preview fit on a single line. Uses currentViewWidth so
        // GetPropertyHeight and OnGUI always agree on the row count.
        static bool FitsOneLine (SerializedProperty property, out string preview, out float previewWidth)
        {
            float[] values =
            {
                property.FindPropertyRelative ("low").floatValue,
                property.FindPropertyRelative ("mid").floatValue,
                property.FindPropertyRelative ("high").floatValue,
            };
            preview = Predict (values);
            previewWidth = EditorStyles.miniLabel.CalcSize (new GUIContent (preview)).x;

            float content = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - LayoutMargin - previewWidth - PreviewGap;
            return content >= OneLineFieldsWidth;
        }

        // Mirrors HardwareTier.Get: sequential mem <= bound checks, skipping any bound that is not above
        // the highest bound seen so far. Produces one "- Tier: range" line per reachable tier.
        static string Predict (float[] tiers)
        {
            string[] names = { "Low", "Mid", "High" };
            var sb = StringBuilderPool.RequestEmpty();

            float maxSeen = 0f;
            bool anyReached = false;

            sb.Append($"- ");

            for (int i = 0; i < 3; i++)
            {
                float upper = tiers[i];
                if (upper <= maxSeen) continue; // this tier is unreachable, skip it

                if( anyReached ) sb.Append($"   |   ");
                string range = anyReached
                    ? $"{FormatGB (maxSeen)} to {FormatGB (upper)}"
                    : $"{FormatGB (upper)} or lower";
                sb.Append($"{names[i]}: {range}");

                maxSeen = upper;
                anyReached = true;
            }

            if( anyReached ) sb.Append($"   |   ");

            sb.Append($"Extreme: {(anyReached ? $"{FormatGB (maxSeen)}+" : "any amount")}");

            return sb.ReturnToPoolAndCast();
        }

        static string FormatGB (float gigabytes) => $"{gigabytes:0.#}GB";
    }
}
