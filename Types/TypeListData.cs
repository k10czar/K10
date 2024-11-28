using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TypeListData
{
    static readonly string[] IGNORED_ASSEMBLY_NAMES = new string[] { "Assembly-CSharp-", "Assembly-CSharp" };

    Type[] _baseTypes = null;
    Type[] _notTypes = null;
    Type[] _newTypes = null;
    Type[] _newTypesWithNull = null;
    string[] _newNames = null;
    string[] _newNamesWithNull = null;
    GUIContent[] _newGUI = null;
    GUIContent[] _newGUIWithNull = null;
    public GUIContent[] EDITOR_newGUI = null;
    public GUIContent[] EDITOR_newGUIWithNull = null;

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
        if( _newGUI != null ) return _newGUI;

        var effectNames = GetNames();
        _newGUI = new GUIContent[effectNames.Length];
        for( int i = 0; i < effectNames.Length; i++ )
        {
            _newGUI[i] = new GUIContent( effectNames[i] );
        } 

        return _newGUI;
    }

    public GUIContent[] GetGUIsWithNull()
    {
        if( _newGUIWithNull != null ) return _newGUIWithNull;

        var guis = GetGUIs();
        _newGUIWithNull = new GUIContent[guis.Length+1];
        _newGUIWithNull[0] = new GUIContent( ConstsK10.NULL_STRING );
        for( int i = 0; i < guis.Length; i++ )
        {
            _newGUIWithNull[i+1] = guis[i];
        } 

        return _newGUIWithNull;
    }

    public string[] GetNames()
    {
        if( _newNames != null ) return _newNames;

        var effectTypes = GetTypes();
        _newNames = new string[effectTypes.Length];
        string commonPart = null;
        for( int i = 0; i < _newNames.Length; i++ )
        {
            var t = effectTypes[i];
            var pathAtt = t.GetCustomAttribute<ListingPathAttribute>();
            if( pathAtt != null ) _newNames[i] = pathAtt.Path;
            else 
            {
                var assemblyName = t.Assembly.GetName().Name;
                foreach( var ignore in IGNORED_ASSEMBLY_NAMES ) 
                    if( assemblyName.StartsWith( ignore ) ) 
                        assemblyName = assemblyName.Substring( ignore.Length );
				if (string.IsNullOrEmpty(assemblyName)) _newNames[i] = t.FullName.Replace(".", "/");
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
					if ( string.IsNullOrEmpty(assemblyNameParsed) ) _newNames[i] = nameParsed;
					else _newNames[i] = assemblyNameParsed + "/" + nameParsed;
				}
            }
            
            var str = _newNames[i];
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
            for( int i = 0; i < _newNames.Length; i++ )
            {
                var str = _newNames[i];
                _newNames[i] = str.Substring( cutLength, str.Length - cutLength );
            }
        }

        return _newNames;
    }

    public string[] GetNameWithNull()
    {
        if( _newNamesWithNull != null ) return _newNamesWithNull;
        
        var notNullNames = GetNames();
        _newNamesWithNull = new string[notNullNames.Length + 1];
        _newNamesWithNull[0] = ConstsK10.NULL_STRING;
        for( int i = 0; i < notNullNames.Length; i++ )
        {
            _newNamesWithNull[i + 1] = notNullNames[i];
        }
        return _newNamesWithNull;
    }

    public Type[] GetTypes()
    {
        if( _newTypes != null ) return _newTypes;

        var ignoreAtt = typeof( HideInInspector );

        _newTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany( s => s.GetTypes() )
                    .Where( p => p.IsPublic && IsAssignableFrom(p) && !p.IsAbstract && IsNotAssignableFrom( p ) && !Attribute.IsDefined( p, ignoreAtt ) ).ToArray();

        return _newTypes;
    }

    public Type[] GetTypesWithNull()
    {
        if( _newTypesWithNull != null ) return _newTypesWithNull;
        
        var notNullTypes = GetTypes();
        _newTypesWithNull = new Type[notNullTypes.Length + 1];
        _newTypesWithNull[0] = null;
        for( int i = 0; i < notNullTypes.Length; i++ )
        {
            _newTypesWithNull[i + 1] = notNullTypes[i];
        }
        return _newTypesWithNull;
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
