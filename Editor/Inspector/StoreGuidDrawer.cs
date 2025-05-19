using K10.EditorGUIExtention;
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

[CustomPropertyDrawer( typeof( StoreGuidFromAttribute ) )]
public class StoreGuidFromDrawer : PropertyDrawer {
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
        return EditorGUI.GetPropertyHeight( property, label, true );
    }

    public override void OnGUI( Rect area, SerializedProperty property, GUIContent label ) {
        var myAttribute = attribute as StoreGuidFromAttribute;
        var guid = property.stringValue;
        var path = AssetDatabase.GUIDToAssetPath( guid );
        var type = myAttribute.TypeRestriction;
        var objRef = AssetDatabase.LoadAssetAtPath( path, type );
        var newRef = ScriptableObjectField.Draw( area, label.text, objRef, type, myAttribute.NewPath, false );
        if( objRef != newRef )
        {
            var newPath = AssetDatabase.GetAssetPath( newRef );
            var newGuid = AssetDatabase.AssetPathToGUID( newPath );
            property.stringValue = newGuid;
        }
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
