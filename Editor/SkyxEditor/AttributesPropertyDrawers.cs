using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Skyx.SkyxEditor
{
    [CustomPropertyDrawer(typeof(SingleLineDrawer))]
    public class SingleLineDrawerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var att = (SingleLineDrawer) attribute;

            var objType = property.GetValue().GetType();
            rect.AdjustToLineAndDivide(att.fields.Length);

            foreach (var (fieldName, color, hint) in att.fields)
            {
                var info = objType.GetField(fieldName);
                SkyxGUI.Draw(rect, property.FindPropertyRelative(fieldName), info.FieldType, color, hint);
                rect.SlideSameRect();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }

    [CustomPropertyDrawer(typeof(LocalPosition))]
    public class LocalPositionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect.AdjustToLine();

            if (!Application.isPlaying)
            {
                var sourceTransform = ((Component) property.serializedObject.targetObject).transform;
                var selectingAnchor = Selection.activeTransform != null &&
                                      Selection.activeTransform.parent == sourceTransform &&
                                      Selection.activeTransform.gameObject.hideFlags == HideFlags.DontSave;

                if (selectingAnchor)
                {
                    if (SkyxGUI.MiniButton(ref rect, "📋", EConsoleColor.Warning, "Copy local position from selected object", true))
                    {
                        property.vector3Value = Selection.activeTransform.localPosition;
                        property.Apply();
                    }

                    if (SkyxGUI.MiniButton(ref rect, "📌", EConsoleColor.Support, "Set local position on selected object", true))
                        Selection.activeTransform.localPosition = property.vector3Value;

                    if (SkyxGUI.MiniButton(ref rect, "❌", EConsoleColor.Support, "Delete helping Anchor", true))
                        Object.DestroyImmediate(Selection.activeObject);
                }
                else
                {
                    if (SkyxGUI.MiniButton(ref rect, "⊙", EConsoleColor.Support, "Create helping anchor", true))
                    {
                        var newObj = new GameObject("[HelpingAnchor]");
                        newObj.transform.parent = sourceTransform;
                        newObj.transform.localPosition = Vector3.zero;
                        newObj.transform.localRotation = Quaternion.identity;
                        newObj.hideFlags = HideFlags.DontSave;

                        Selection.activeObject = newObj;
                    }
                }
            }

            EditorGUI.PropertyField(rect, property, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }
}