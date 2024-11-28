using UnityEngine;
using UnityEditor;
using K10.EditorGUIExtention;

[CustomEditor(typeof(TagsDebug))]
public class TagsDebugEditor : Editor
{
    Vector2 _scroll;
    bool _showRefs = false;

    private TagsDebug _target => target as TagsDebug;

    public override void OnInspectorGUI()
    {
        var guidsProp = serializedObject.FindProperty( "_guids" );
        var namesProp = serializedObject.FindProperty( "_names" );

        SeparationLine.Horizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{guidsProp.arraySize} Tags", K10GuiStyles.bigBold, GUILayout.Height( 40 ));
        if( GUILayout.Button("Rescan", K10GuiStyles.bigbuttonStyle, GUILayout.Width( 100 )) ) _target.Rebuild();
        EditorGUILayout.EndHorizontal();

        SeparationLine.Horizontal();
        if( guidsProp.arraySize != namesProp.arraySize )
        {
            EditorGUILayout.LabelField( $"ERROR: guidsProp.arraySize( {guidsProp.arraySize} ) != namesProp.arraySize( {namesProp.arraySize} )" );
        }
        
        var nameWidth = 10f;
        for( int i = 0; i < namesProp.arraySize; i++ )
        {
            var name = namesProp.GetArrayElementAtIndex(i).stringValue;
            var content = new GUIContent(name);
            var style = GUI.skin.textField;
            var size = style.CalcSize(content);
            var width = size.x + 10;
            nameWidth = width > nameWidth ? width : nameWidth;
        }
        
        var guidWidth = 10f;
        for( int i = 0; i < guidsProp.arraySize; i++ )
        {
            var guid = guidsProp.GetArrayElementAtIndex(i).stringValue;
            var content = new GUIContent(guid);
            var style = GUI.skin.textField;
            var size = style.CalcSize(content);
            var width = size.x + 10;
            guidWidth = width > guidWidth ? width : guidWidth;
        }

        var nameWidthProp = GUILayout.Width( nameWidth );
        var guidWidthProp = GUILayout.Width( guidWidth );

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup( true );
        GUILayout.Button( "Name", nameWidthProp );
        GUILayout.Button( "GUID", guidWidthProp );
        EditorGUI.EndDisabledGroup();
        var toggleRefShow = GUILayout.Button( _showRefs ? "Hide References" : "Show References" );
        if( toggleRefShow ) _showRefs = !_showRefs;
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView( _scroll );
        for( int i = 0; i < guidsProp.arraySize || i < namesProp.arraySize; i++ )
        {
            var name = namesProp.arraySize > i ? namesProp.GetArrayElementAtIndex(i).stringValue : "OUT_OF_RANGE";
            var guid = guidsProp.arraySize > i ? guidsProp.GetArrayElementAtIndex(i).stringValue : "OUT_OF_RANGE";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField( name, nameWidthProp );
            EditorGUILayout.TextField( guid, guidWidthProp );
            if( _showRefs )
            {
                var asset = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( guid ), typeof(Object) );
                EditorGUILayout.ObjectField( asset, asset != null ? asset.GetType() : typeof(Object), false );
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
