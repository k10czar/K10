using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( HsoRef<> ), true )]
public class HsoRefDrawer : PropertyDrawer
{
	static readonly Dictionary<Type, IHashedSOCollection> _collectionCache = new();
	[ConstLike] static readonly Color _brokenColor = Color.Lerp( Color.red, Color.white, .5f );

	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		var hashIdProp = property.FindPropertyRelative( "_referenceHashID" );
		if( hashIdProp == null ) { EditorGUI.LabelField( position, label, EditorGUIUtility.TrTextContent( "Missing _referenceHashID" ) ); return; }

		var hsoRefType = UnwrapCollectionType( fieldInfo.FieldType );
		var tType = hsoRefType.GetGenericArguments()[0];
		var collection = GetCollection( hsoRefType );
		var currentId = hashIdProp.intValue;
		var current = currentId != -1 ? collection?.GetElementBase( currentId ) as HashedScriptableObject : null;
		var broken = currentId != -1 && current == null;

		if( broken ) GuiColorManager.New( _brokenColor );
		EditorGUI.BeginProperty( position, label, property );
		EditorGUI.BeginChangeCheck();
		var newObj = EditorGUI.ObjectField( position, label, current, tType, false ) as HashedScriptableObject;
		if( EditorGUI.EndChangeCheck() )
		{
			var newId = newObj != null ? newObj.HashID : -1;
			Debug.Log( $"Changed object to {newObj} id changed from {hashIdProp.intValue} to {newId}" );
			hashIdProp.intValue = newObj != null ? newObj.HashID : -1;
		}
		EditorGUI.EndProperty();
		if( broken ) GuiColorManager.Revert();
	}

	private static Type UnwrapCollectionType( Type type )
	{
		if( type.IsArray ) return type.GetElementType();
		if( type.IsGenericType ) return type.GetGenericArguments()[0];
		return type;
	}

	private static IHashedSOCollection GetCollection( Type hsoRefType )
	{
		if( _collectionCache.TryGetValue( hsoRefType, out var cached ) ) return cached;
		var dummyField = hsoRefType.GetProperty( "DummyInstance", BindingFlags.Static | BindingFlags.NonPublic );
		var dummy = dummyField?.GetValue( null ) as HashedScriptableObject;
		var collection = dummy?.GetCollection();
		if( collection != null ) _collectionCache[hsoRefType] = collection;
		return collection;
	}
}
