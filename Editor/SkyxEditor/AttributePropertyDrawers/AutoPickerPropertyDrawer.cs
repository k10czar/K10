using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rogue.REditor
{
    [CustomPropertyDrawer(typeof(AutoPickerAttribute))]
    public class AutoPickerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var color = property.propertyType is SerializedPropertyType.ObjectReference && property.objectReferenceValue != null
                ? EColor.Support : EColor.Special;

            if (SkyxGUI.MiniButton(ref rect, "👆", color, "Auto Pick", true))
            {
                var pickerAtt = (AutoPickerAttribute) attribute;
                var targetType = property.GetObjectReferenceType();

                if (pickerAtt.getterMethod == null)
                    property.FillWithExisting(targetType, pickerAtt.searchChildren, pickerAtt.searchParent);
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

                if (pickerAtt.addIfNotFound && property.objectReferenceValue == null)
                {
                    var behaviour = (MonoBehaviour) property.serializedObject.targetObject;
                    var component = behaviour.gameObject.AddComponent(targetType);
                    property.objectReferenceValue = component;
                    property.Apply();
                }
            }

            EditorGUI.PropertyField(rect, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => SkyxStyles.LineHeight;
    }
}