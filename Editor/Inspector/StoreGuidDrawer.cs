using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( StoreGuidAttribute ) )]
public class StoreGuidDrawer : PropertyDrawer {
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        return EditorGUI.GetPropertyHeight( property, label, true );
    }

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label ) {
        StoreGuid.At( property );
        using( var scope = new EditorGUI.DisabledGroupScope(true) ) EditorGUI.PropertyField( area, property, label, true );
    }
}

[CustomPropertyDrawer( typeof( StoreFileIDAttribute ) )]
public class StoreFileIDDrawer : PropertyDrawer {
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        return EditorGUI.GetPropertyHeight( property, label, true );
    }

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label ) {
        StoreFileID.At( property );
        using( var scope = new EditorGUI.DisabledGroupScope(true) ) EditorGUI.PropertyField( area, property, label, true );
    }
}
