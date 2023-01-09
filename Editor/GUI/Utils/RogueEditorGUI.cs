using UnityEditor;
using UnityEngine;

using Prop = UnityEditor.SerializedProperty;

public static class RogueEditorGUI
{
    public static string[] CreateEnumNamesArray<T>( bool breakUnderline = true )
    {
        var names = System.Enum.GetNames( typeof( T ) );
        if( !breakUnderline ) return names;
        for( int i = 0; i < names.Length; i++ ) { names[i] = names[i].Replace( '_', '/' ); }
        return names;
    }

	public static class CachedEnumNames<T> where T : struct, System.IConvertible
	{
		static string[] _names, _breakedUnderline;

		public static string[] Names
		{
			get
			{
				if( _names == null ) _names = CreateEnumNamesArray<T>( false );
				return _names;
			}
		}

		public static string[] BreakedUnderline
		{
			get
			{
				if( _breakedUnderline == null ) _breakedUnderline = CreateEnumNamesArray<T>( true );
				return _breakedUnderline;
			}
		}

		public static string[] Get( bool breakUnderline = true ) { return breakUnderline ? BreakedUnderline : Names; }
	}

    public static void Tag( Rect rect, string tag ) { if( !string.IsNullOrEmpty( tag ) ) GUI.Label( rect, tag, K10GuiStyles.unitStyle ); }

    public static void PropertyField( Rect rect, Prop prop, string tag )
    {
        EditorGUI.PropertyField( rect, prop, new GUIContent( default( string ), tag ) );
        Tag( rect, tag );
    }

    public static void PropertyField( Rect rect, Prop prop, string tag, Color guiColor )
    {
		GuiColorManager.New( guiColor );
        PropertyField( rect, prop, tag );
        GuiColorManager.Revert();
    }

    public static void Enum<T>( Rect rect, Prop prop, string tag = null, bool breakUnderline = true ) where T : struct, System.IConvertible
    {
        prop.enumValueIndex = EditorGUI.Popup( rect, prop.enumValueIndex, CachedEnumNames<T>.Get( breakUnderline ) );
        Tag( rect.CutRight( 10 ), tag );
	}

    public static void Enum<T>( Rect rect, Prop prop, Color guiColor, string tag = null, bool breakUnderline = true ) where T : struct, System.IConvertible
    {
		GuiColorManager.New( guiColor );
        Enum<T>( rect, prop, tag, breakUnderline );
        GuiColorManager.Revert();
    }

    public static void EnumInt<T>( Rect rect, Prop prop, string tag = null, bool breakUnderline = true ) where T : struct, System.IConvertible
    {
        prop.intValue = EditorGUI.Popup( rect, prop.intValue, CachedEnumNames<T>.Get( breakUnderline ) );
        Tag( rect.CutRight( 10 ), tag );
    }

    public static void EnumInt<T>( Rect rect, Prop prop, Color guiColor, string tag = null, bool breakUnderline = true ) where T : struct, System.IConvertible
    {
        GuiColorManager.New( guiColor );
        EnumInt<T>( rect, prop, tag, breakUnderline );
        GuiColorManager.Revert();
    }

    public static void EnumMask<T>( Rect rect, Prop prop, string tag = null ) where T : struct, System.IConvertible
    {
        prop.enumValueIndex = EditorGUI.MaskField( rect, prop.enumValueIndex, CachedEnumNames<T>.Get( false ) );
        Tag( rect.CutRight( 10 ), tag );
    }

    public static void EnumMask<T>( Rect rect, Prop prop, Color guiColor, string tag = null ) where T : struct, System.IConvertible
    {
        GuiColorManager.New( guiColor );
        EnumMask<T>( rect, prop, tag );
        GuiColorManager.Revert();
    }

    public static void EnumIntMask<T>( Rect rect, Prop prop, string tag = null ) where T : struct, System.IConvertible
    {
        prop.intValue = EditorGUI.MaskField( rect, prop.intValue, CachedEnumNames<T>.Get( false ) );
        Tag( rect.CutRight( 10 ), tag );
    }

    public static void EnumIntMask<T>( Rect rect, Prop prop, Color guiColor, string tag = null ) where T : struct, System.IConvertible
    {
        GuiColorManager.New( guiColor );
        EnumIntMask<T>( rect, prop, tag );
        GuiColorManager.Revert();
    }
}