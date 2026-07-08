using System.Collections.Generic;
using Rogue.RuntimeEditor;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(WeightedDrawerAttribute))]
    public class WeightedDrawerAttributePropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<(int, string), (int, float)> sumCache = new();

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var att = (WeightedDrawerAttribute)attribute;
            rect.AdjustToLineAndDivide(att.fieldNames.Length);

            var changed = false;
            EditorGUI.BeginChangeCheck();

            for (var index = 0; index < att.fieldNames.Length - 1; index++)
            {
                var fieldName = att.fieldNames[index];
                var innerProp = property.FindPropertyRelative(fieldName);
                EditorGUI.PropertyField(rect, innerProp, GUIContent.none);
                rect.SlideSame();
            }

            var weightFieldName = att.fieldNames[^1];
            var weightProp = property.FindPropertyRelative(weightFieldName);

            if (weightProp.floatValue < 0)
            {
                weightProp.floatValue = 0;
                changed = true;
            }

            rect.DivideRect(2);
            EditorGUI.PropertyField(rect, weightProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck() || changed)
            {
                property.Apply();
                changed = true;
            }

            var sum = CalculateListWeightSum(property, weightFieldName, changed);
            var percent = Mathf.Approximately(sum, 0) ? 0 : weightProp.floatValue / sum;

            rect.SlideSame();
            EditorGUI.ProgressBar(rect, percent, percent.ToPercentageString());
        }

        private static float CalculateListWeightSum(SerializedProperty property, string weightFieldName, bool force)
        {
            var parentProperty = property.FindParentProperty();
            var cacheID = parentProperty.GetCacheID();

            if (!parentProperty.isArray)
            {
                Debug.LogError("Weighted Drawer can only be used on an List/Array!");
                return 0;
            }

            if (!force && sumCache.TryGetValue(cacheID, out var entry))
            {
                var (prevCount, sum) = entry;
                if (prevCount == parentProperty.arraySize)
                    return sum;
            }

            float newSum = 0;
            for (var i = 0; i < parentProperty.arraySize; i++)
                newSum += parentProperty.GetArrayElementAtIndex(i).FindPropertyRelative(weightFieldName).floatValue;

            sumCache[cacheID] = (parentProperty.arraySize, newSum);

            return newSum;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}