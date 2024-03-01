using System.Reflection;
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
            var iconName = effectTypes[i].GetCustomAttribute<OverridingIconAttribute>()?.Path ?? null;
            if( iconName != null ) guis[i] = new GUIContent( effectNames[i], IconCache.Get( iconName ).Texture );
            else guis[i] = new GUIContent( effectNames[i] );
        } 

        return guis;
    }
}
