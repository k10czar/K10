using System.Reflection;
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

    [CustomPropertyDrawer(typeof(LocalPositionAttribute))]
    public class LocalPositionPropertyDrawer : PropertyDrawer
    {
        private const string AnchorName = "[HelpingAnchor]";

        private static void OpenMenuPicker(SerializedProperty property, Transform sourceTransform, int validMask)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Get Existing Anchor"), false, () => SelectExistingAnchor(sourceTransform));
            menu.AddItem(new GUIContent("Create anchor at current position"), false, () => CreateSelectingAnchor(sourceTransform, property.vector3Value));
            menu.AddItem(new GUIContent("Create anchor at Click"), false, () => EditorUtils.RunOnSceneClick(hit => CreateSelectingAnchor(sourceTransform, sourceTransform.InverseTransformPoint(hit.point)), validMask));

            menu.ShowAsContext();
        }

        private static void SelectExistingAnchor(Transform sourceTransform)
        {
            var anchor = sourceTransform.Find(AnchorName);
            if (anchor == null)
            {
                Debug.LogError($"Cannot find anchor {AnchorName}");
                return;
            }

            EditorUtils.SetInspectorLock(true);
            Selection.activeObject = anchor;
        }

        private static void CreateSelectingAnchor(Transform sourceTransform, Vector3 localPosition)
        {
            EditorUtils.SetInspectorLock(true);

            var anchor = new GameObject("[HelpingAnchor]")
            {
                transform =
                {
                    parent = sourceTransform,
                    localPosition = localPosition,
                    localRotation = Quaternion.identity
                },
                hideFlags = HideFlags.DontSave
            };

            Selection.activeObject = anchor;
        }

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
                    if (SkyxGUI.MiniButton(ref rect, "📋", EColor.Warning, "Copy local position from selected object", true))
                    {
                        property.vector3Value = Selection.activeTransform.localPosition;
                        property.Apply();
                    }

                    if (SkyxGUI.MiniButton(ref rect, "📌", EColor.Support, "Set local position on selected object", true))
                        Selection.activeTransform.localPosition = property.vector3Value;

                    if (SkyxGUI.MiniButton(ref rect, "❌", EColor.Support, "Delete helping Anchor", true))
                        Object.DestroyImmediate(Selection.activeObject);
                }
                else
                {
                    if (SkyxGUI.MiniButton(ref rect, "⊙", EColor.Support, "Create helping anchor", true))
                        OpenMenuPicker(property, sourceTransform, ((LocalPositionAttribute)attribute).validMask);
                }
            }

            EditorGUI.PropertyField(rect, property, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.CompactListElement;
    }

    [CustomPropertyDrawer(typeof(AutoPickerAttribute))]
    public class AutoPickerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var color = property.propertyType is SerializedPropertyType.ObjectReference && property.objectReferenceValue != null
                ? EColor.Support : EColor.Special;

            if (SkyxGUI.MiniButton(ref rect, "⚙️", color, "Auto Pick", true))
            {
                var pickerAtt = (AutoPickerAttribute) attribute;

                if (pickerAtt.getterMethod == null)
                {
                    var targetType = property.GetObjectReferenceType();
                    property.FillWithExisting(targetType, pickerAtt.searchChildren, pickerAtt.searchParent);
                }
                else
                {
                    var ownerType = property.serializedObject.targetObject.GetType();
                    var method = ownerType.GetMethod(pickerAtt.getterMethod, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    var parent = property.GetParentValue(ownerType);
                    var result = method!.Invoke(parent, null);

                    property.PrepareForChanges("Auto picker");
                    property.SetValue(result);
                    property.ApplyDirectChanges();
                }
            }

            EditorGUI.PropertyField(rect, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }

    [CustomPropertyDrawer(typeof(OptionalAttribute))]
    public class OptionalAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var optionalAtt = (OptionalAttribute) attribute;

            if (optionalAtt.showLabel)
                EditorGUI.LabelField(rect.ExtractLabelRect(), label);

            var isFloat = property.propertyType is SerializedPropertyType.Float;
            var currentValue = isFloat ? property.floatValue : property.intValue;

            if (currentValue < 0)
            {
                if (SkyxGUI.Button(rect, optionalAtt.compact))
                {
                    if (isFloat) property.floatValue = 1;
                    else property.intValue = 1;
                    property.Apply();
                }
            }
            else
            {
                if (SkyxGUI.MiniButton(ref rect, "!", EColor.Support, optionalAtt.hint, true))
                {
                    if (isFloat) property.floatValue = -1;
                    else property.intValue = -1;
                    property.Apply();
                }

                EditorGUI.PropertyField(rect, property, GUIContent.none);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}