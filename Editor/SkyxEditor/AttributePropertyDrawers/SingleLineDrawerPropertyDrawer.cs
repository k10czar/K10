using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(SingleLineDrawerAttribute))]
    public class SingleLineDrawerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var att = (SingleLineDrawerAttribute) attribute;

            var objType = property.GetValue().GetType();
            rect.AdjustToLineAndDivide(att.drawInfos.Length);

            foreach (var drawInfo in att.drawInfos)
            {
                var info = objType.GetField(drawInfo.fieldName);
                SkyxGUI.Draw(rect, property.FindPropertyRelative(drawInfo.fieldName), info.FieldType, drawInfo);
                rect.SlideSame();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}