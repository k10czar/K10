using UnityEngine;
using UnityEditor;

public static class SortingFieldEditorExtension
{
    public static void LayoutAsButtonList<T>( this SortingField<T> sortingField )
    {
        sortingField.UpdateIfDirty();
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        GuiUtils.Label.ExactSizeLayout( "Sort:" );

        var names = sortingField.CachedNames;

        for( int i = 0; i < names.Length; i++ ) 
        {
            if( GuiUtils.Button.ExactSizeLayout( names[i] ) )
                sortingField.Toggle( i );
        }
        EditorGUILayout.EndHorizontal();
    }
    
    public static void LayoutAsDropdown<T>( this SortingField<T> sortingField, params GUILayoutOption[] opts )
    {
        sortingField.UpdateIfDirty();
        EditorGUILayout.BeginHorizontal( GUI.skin.box, opts );
        GuiUtils.Label.ExactSizeLayout( "Sort:" );
        // EditorGUI.BeginChangeCheck();
        var names = sortingField.CachedNames;
        var id = EditorGUILayout.Popup( 0, names );
        // var changed = EditorGUI.EndChangeCheck();
        if( GuiUtils.Button.ExactSizeLayout( "↕" ) || id != 0 /*|| changed*/ ) sortingField.Toggle( id );
        EditorGUILayout.EndHorizontal();
    }
}
