using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

public static class CustomEditorUtility
{
    private static MethodInfo _getEditorTypeMethod;
    private static LimitedCache<UnityEngine.Object,Editor> _cache = new( TimeSpan.FromMinutes( 5 ), 100 );

    static CustomEditorUtility()
    {
        var type = typeof(Editor).Assembly.GetType(
            "UnityEditor.CustomEditorAttributes"
        );

        _getEditorTypeMethod = type?.GetMethod(
            "FindCustomEditorType",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(UnityEngine.Object), typeof(bool) },
            null
        );
    }

	public static Type GetEditorType(UnityEngine.Object target, bool multiEdit = false)
    {
        if (_getEditorTypeMethod == null || target == null)
            return null;

        return _getEditorTypeMethod.Invoke(
            null,
            new object[] { target, multiEdit }
        ) as Type;
    }

    public static Editor GetEditor(UnityEngine.Object target, bool multiEdit = false)
    {
		if( target == null ) return null;

		if( _cache.TryGetValue( target, out Editor editor) ) return editor;

        if (_getEditorTypeMethod == null)
        {
            Debug.Log( "_getEditorTypeMethod == null" );   
            return null;
        }

		var editorType = GetEditorType( target, multiEdit );

		if( editorType == null ) 
		{
            Debug.Log( "editorType == null" );   
			_cache.Add( target, null );
			return null;
		}
		
		var customEditor = Editor.CreateEditor( target, editorType );
		_cache.Add( target, customEditor );
		return customEditor;
    }
}
