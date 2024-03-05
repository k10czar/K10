using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class TypeListDataExtension
{
    public static GUIContent[] GetGUIsWithIcon( this TypeListData list )
    {
        var guis = list.EDITOR_newSkillEffectGUI;
        if( guis != null ) return guis;

        var effectNames = list.GetNames();
        guis = list.EDITOR_newSkillEffectGUI = new GUIContent[effectNames.Length];
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
}
