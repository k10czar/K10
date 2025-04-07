using UnityEngine;

public static class TypeListDataExtension
{
    public static GUIContent[] GetGUIsWithIcon( this TypeListData list )
    {
        var guis = list.EDITOR_newGUI;
        if( guis != null ) return guis;

        var effectNames = list.GetNames();
        guis = list.EDITOR_newGUI = new GUIContent[effectNames.Length];
        var effectTypes = list.GetTypes();
        for( int i = 0; i < effectNames.Length; i++ )
        {
            var type = effectTypes[i];
            var texture = type.EditorGetIcon();
            if( texture != null ) guis[i] = new GUIContent( effectNames[i], texture );
            else guis[i] = new GUIContent( effectNames[i] );
        }

        return guis;
    }
    public static GUIContent[] GetGUIsWithIconWithNull( this TypeListData list )
    {
        if( list.EDITOR_newGUIWithNull != null ) return list.EDITOR_newGUIWithNull;

        var guis = GetGUIsWithIcon( list );
        list.EDITOR_newGUIWithNull = new GUIContent[guis.Length + 1];
        list.EDITOR_newGUIWithNull[0] = new GUIContent( ConstsK10.NULL_STRING );
        for( int i = 0; i < guis.Length; i++ ) list.EDITOR_newGUIWithNull[i + 1] = guis[i];
        return list.EDITOR_newGUIWithNull;
    }
}