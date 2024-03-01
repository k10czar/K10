using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TypeListData
{
    Type _baseType = null;
    Type[] _newSkillEffectTypes = null;
    string[] _newSkillEffectNames = null;
    GUIContent[] _newSkillEffectGUI = null;
    public GUIContent[] EDITOR_newSkillEffectGUI = null;

    public TypeListData( System.Type type )
    {
        _baseType = type;
    }

    public GUIContent[] GetGUIs()
    {
        if( _newSkillEffectGUI != null ) return _newSkillEffectGUI;

        var effectNames = GetNames();
        _newSkillEffectGUI = new GUIContent[effectNames.Length];
        for( int i = 0; i < effectNames.Length; i++ )
        {
            _newSkillEffectGUI[i] = new GUIContent( effectNames[i] );
        } 

        return _newSkillEffectGUI;
    }

    public string[] GetNames()
    {
        if( _newSkillEffectNames != null ) return _newSkillEffectNames;

        var effectTypes = GetTypes();
        _newSkillEffectNames = new string[effectTypes.Length];
        for( int i = 0; i < _newSkillEffectNames.Length; i++ ) 
        {
            var t = effectTypes[i];
            var pathAtt = t.GetCustomAttribute<ListingPathAttribute>();
            if( pathAtt != null ) _newSkillEffectNames[i] = pathAtt.Path;
            else _newSkillEffectNames[i] = t.FullName;
        }

        return _newSkillEffectNames;
    }

    public Type[] GetTypes()
    {
        if( _newSkillEffectTypes != null ) return _newSkillEffectTypes;

        _newSkillEffectTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany( s => s.GetTypes() )
                    .Where( p => _baseType.IsAssignableFrom(p) && !p.IsAbstract ).ToArray();

        return _newSkillEffectTypes;
    }
    
    float _maxWidth = -1;
    public float MaxWidth
    {
        get
        {
#if UNITY_EDITOR
            if( _maxWidth > -Mathf.Epsilon ) return _maxWidth;

            _maxWidth = 10;
            foreach( var lab in GetGUIs() )
            {
                UnityEditor.EditorStyles.label.CalcMinMaxWidth( lab, out var minW, out var maxW );
                maxW += 15;
                if( _maxWidth < maxW ) _maxWidth = maxW;
            }
            return _maxWidth;
#else
            return 0;
#endif
        }
    }
}
