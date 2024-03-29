using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TypeListData
{
    static readonly string[] IGNORED_ASSEMBLY_NAMES = new string[] { "Assembly-CSharp-", "Assembly-CSharp" };

    Type[] _baseTypes = null;
    Type[] _notTypes = null;
    Type[] _newSkillEffectTypes = null;
    string[] _newSkillEffectNames = null;
    GUIContent[] _newSkillEffectGUI = null;
    public GUIContent[] EDITOR_newSkillEffectGUI = null;

    public TypeListData( params System.Type[] types )
    {
        _baseTypes = types;
        var uObj = typeof(UnityEngine.Object);
        _notTypes = new Type[]{ uObj };
        foreach( var t in types )
        {
            if( !uObj.IsAssignableFrom( t ) ) continue;
            _notTypes = null;
            break;
        }
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
        string commonPart = null;
        for( int i = 0; i < _newSkillEffectNames.Length; i++ )
        {
            var t = effectTypes[i];
            var pathAtt = t.GetCustomAttribute<ListingPathAttribute>();
            if( pathAtt != null ) _newSkillEffectNames[i] = pathAtt.Path;
            else 
            {
                var assemblyName = t.Assembly.GetName().Name;
                foreach( var ignore in IGNORED_ASSEMBLY_NAMES ) 
                    if( assemblyName.StartsWith( ignore ) ) 
                        assemblyName = assemblyName.Substring( ignore.Length );
				if (string.IsNullOrEmpty(assemblyName)) _newSkillEffectNames[i] = t.FullName.Replace(".", "/");
				else
				{
					var assemblyNameParsed = assemblyName.Replace(".", "/");
					var nameParsed = t.FullName.Replace(".", "/");
					var equalId = 0;
					for( ; equalId < assemblyNameParsed.Length && equalId < nameParsed.Length; equalId++ )
					{
						if (assemblyNameParsed[equalId] != nameParsed[equalId]) break;
					}
					assemblyNameParsed = assemblyNameParsed.Substring(0, equalId);
					if ( string.IsNullOrEmpty(assemblyNameParsed) ) _newSkillEffectNames[i] = nameParsed;
					else _newSkillEffectNames[i] = assemblyNameParsed + "/" + nameParsed;
				}
            }
            
            var str = _newSkillEffectNames[i];
            if( commonPart == null ) 
            {
                int id = -1;
                for( int si = str.Length - 1; si >= 0; si-- ) if( str[si] == '/' ) { id = si; break; }
                if( id == -1 ) commonPart = string.Empty;
                else commonPart = str.Substring( 0, id + 1 );
            }
            else
            {
                int id = -1;
                for( int si = 0; si < str.Length && si < commonPart.Length; si++ ) if( commonPart[si] == str[si] ) { id = si; } else break;
                for( ; id >= 0; id-- ) if( commonPart[id] == '/' ) break;
                if( id == -1 ) commonPart = string.Empty;
                else commonPart = str.Substring( 0, id + 1 );
            }
        }

        if( !string.IsNullOrEmpty( commonPart ) )
        {
            var cutLength = commonPart.Length;
            for( int i = 0; i < _newSkillEffectNames.Length; i++ )
            {
                var str = _newSkillEffectNames[i];
                _newSkillEffectNames[i] = str.Substring( cutLength, str.Length - cutLength );
            }
        }

        return _newSkillEffectNames;
    }

    public Type[] GetTypes()
    {
        if( _newSkillEffectTypes != null ) return _newSkillEffectTypes;

        _newSkillEffectTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany( s => s.GetTypes() )
                    .Where( p => p.IsPublic && IsAssignableFrom(p) && !p.IsAbstract && IsNotAssignableFrom( p ) ).ToArray();

        return _newSkillEffectTypes;
    }

    private bool IsAssignableFrom( Type t )
    {
        foreach( var bt in _baseTypes )
        {
            if( !bt.IsAssignableFrom( t ) ) return false;
        }
        return true;
    }

    private bool IsNotAssignableFrom( Type t )
    {
        if( _notTypes == null ) return true;
        foreach( var nt in _notTypes )
        {
            if( nt.IsAssignableFrom( t ) ) return false;
        }
        return true;
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

    public object BaseTypesDebug => string.Join( ",", _baseTypes.ToList().ConvertAll( ( t ) => t.Name ).ToArray() );
}
