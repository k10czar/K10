using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( ToStringAttribute ) )]
public sealed class ToStringDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUIUtility.singleLineHeight;
    }

    private static string ToStringValue( SerializedProperty property )
    {
        switch (property.propertyType)
        {
            // case SerializedPropertyType.Generic: return property..ToStringOrNull();
            case SerializedPropertyType.Integer: return property.intValue.ToString();
            case SerializedPropertyType.Boolean: return property.boolValue.ToString();
            case SerializedPropertyType.Float: return property.floatValue.ToString();
            case SerializedPropertyType.String: return property.stringValue.ToStringOrNull();
            case SerializedPropertyType.Color: return property.colorValue.ToString();
            case SerializedPropertyType.ObjectReference: return property.objectReferenceValue.ToStringOrNull();
            // case SerializedPropertyType.LayerMask: return property..ToStringOrNull();
            case SerializedPropertyType.Enum: return property.enumValueIndex.ToStringOrNull();
            case SerializedPropertyType.Vector2: return property.vector2Value.ToStringOrNull();
            case SerializedPropertyType.Vector3: return property.vector3Value.ToStringOrNull();
            case SerializedPropertyType.Vector4: return property.vector4Value.ToStringOrNull();
            case SerializedPropertyType.Rect: return property.rectValue.ToStringOrNull();
            case SerializedPropertyType.ArraySize: return property.arraySize.ToStringOrNull();
            // case SerializedPropertyType.Character: return property..ToStringOrNull();
            case SerializedPropertyType.AnimationCurve: return property.animationCurveValue.ToStringOrNull();
            case SerializedPropertyType.Bounds: return property.boundsValue.ToStringOrNull();
            // case SerializedPropertyType.Gradient: return property.gradientValue.ToStringOrNull();
            case SerializedPropertyType.Quaternion: return property.quaternionValue.ToStringOrNull();
            case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue.ToStringOrNull();
            case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize.ToStringOrNull();
            case SerializedPropertyType.Vector2Int: return property.vector2IntValue.ToStringOrNull();
            case SerializedPropertyType.Vector3Int: return property.vector3IntValue.ToStringOrNull();
            case SerializedPropertyType.RectInt: return property.rectIntValue.ToStringOrNull();
            case SerializedPropertyType.BoundsInt: return property.boundsIntValue.ToStringOrNull();
            case SerializedPropertyType.ManagedReference: return property.managedReferenceValue.ToStringOrNull();
            case SerializedPropertyType.Hash128: return property.hash128Value.ToStringOrNull();
        }
        return "NOT IMPLEMENTED";
    }

    public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
    {
        GUI.Label( rect, $"{property.name}: {ToStringValue(property)}" );
    }
}
