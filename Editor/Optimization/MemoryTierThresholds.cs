using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace K10.Optimization.Editor
{
    // The three memory (MB) upper bounds that drive HardwareTier: mem <= low => Low, <= mid => Mid,
    // <= high => High, otherwise Extreme. A bound that is not greater than the running maximum simply
    // skips that tier (e.g. { 6000, 0, 0 } yields only Low and Extreme).
    [Serializable]
    public class MemoryTierThresholds
    {
        [Tooltip ("Upper bound (MB) for the Low tier.")] public int low = 6000;
        [Tooltip ("Upper bound (MB) for the Mid tier. Set it above Low to enable Mid.")] public int mid = 0;
        [Tooltip ("Upper bound (MB) for the High tier. Set it above Mid to enable High.")] public int high = 0;

        public int[] ToArray () => new[] { low, mid, high };
    }

    [CustomPropertyDrawer (typeof (MemoryTierThresholds))]
    public class MemoryTierThresholdsDrawer : PropertyDrawer
    {
        static readonly GUIContent[] SubLabels = { new GUIContent ("Low"), new GUIContent ("Mid"), new GUIContent ("High") };
        const float Spacing = 2f;
        const float PreviewIndent = 12f;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            int previewLines = Predict (Read (property)).Count;
            int rows = 1 + previewLines; // the fields row + one row per predicted tier
            return rows * EditorGUIUtility.singleLineHeight + (rows - 1) * Spacing;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty low = property.FindPropertyRelative ("low");
            SerializedProperty mid = property.FindPropertyRelative ("mid");
            SerializedProperty high = property.FindPropertyRelative ("high");

            float h = EditorGUIUtility.singleLineHeight;

            // Row 1: prefix label + the three bounds as a compact multi-int field.
            Rect fieldsRow = new Rect (position.x, position.y, position.width, h);
            Rect fields = EditorGUI.PrefixLabel (fieldsRow, label);

            int[] values = { low.intValue, mid.intValue, high.intValue };
            EditorGUI.BeginChangeCheck ();
            EditorGUI.MultiIntField (fields, SubLabels, values);
            if (EditorGUI.EndChangeCheck ())
            {
                low.intValue = values[0];
                mid.intValue = values[1];
                high.intValue = values[2];
            }

            // Following rows: predicted reachable tiers and their memory ranges.
            float y = position.y + h + Spacing;
            foreach (string line in Predict (values))
            {
                Rect r = new Rect (position.x + PreviewIndent, y, position.width - PreviewIndent, h);
                EditorGUI.LabelField (r, line, EditorStyles.miniLabel);
                y += h + Spacing;
            }
        }

        static int[] Read (SerializedProperty property) => new[]
        {
            property.FindPropertyRelative ("low").intValue,
            property.FindPropertyRelative ("mid").intValue,
            property.FindPropertyRelative ("high").intValue,
        };

        // Mirrors HardwareTier.Get: sequential mem <= bound checks, skipping any bound that is not above
        // the highest bound seen so far. Produces one "- Tier: range" line per reachable tier.
        static List<string> Predict (int[] tiers)
        {
            string[] names = { "Low", "Mid", "High" };
            List<string> lines = new List<string> ();

            long maxSeen = 0;
            bool anyReached = false;

            for (int i = 0; i < 3; i++)
            {
                int upper = tiers[i];
                if (upper <= maxSeen) continue; // this tier is unreachable, skip it

                string range = anyReached
                    ? $"{FormatGB ((int) maxSeen)} to {FormatGB (upper)}"
                    : $"{FormatGB (upper)} or lower";
                lines.Add ($"- {names[i]}: {range}");

                maxSeen = upper;
                anyReached = true;
            }

            lines.Add ($"- Extreme: {(anyReached ? $"{FormatGB ((int) maxSeen)}+" : "any amount")}");
            return lines;
        }

        static string FormatGB (int megabytes)
        {
            return megabytes % 1000 == 0
                ? $"{megabytes / 1000}GB"
                : $"{megabytes / 1000f:0.#}GB";
        }
    }
}
