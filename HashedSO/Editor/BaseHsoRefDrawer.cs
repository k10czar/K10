using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( BaseHsoRef<> ), true )]
public class BaseHsoRefDrawer : PropertyDrawer
{
	static readonly Dictionary<Type, IHashedSOCollection> _collectionCache = new();
	[ConstLike] static readonly Color _brokenColor = Color.Lerp( Color.red, Color.white, .5f );

	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		var hashIdProp = property.FindPropertyRelative( "_referenceHashID" );
		if( hashIdProp == null ) { EditorGUI.LabelField( position, label, EditorGUIUtility.TrTextContent( "Missing _referenceHashID" ) ); return; }

		var hsoRefType = UnwrapCollectionType( fieldInfo.FieldType );
		var tType = GetElementType( hsoRefType );
		if( tType == null ) { EditorGUI.LabelField( position, label, EditorGUIUtility.TrTextContent( $"Cannot resolve T from {hsoRefType.Name}" ) ); return; }
		var collection = GetCollection( tType );
		var currentId = hashIdProp.intValue;
		var current = currentId != -1 ? collection?.GetElementBase( currentId ) as HashedScriptableObject : null;
		var broken = currentId != -1 && current == null;

		if( broken ) GuiColorManager.New( _brokenColor );
		EditorGUI.BeginProperty( position, label, property );
		EditorGUI.BeginChangeCheck();
		var displayLabel = new GUIContent( $"{label.text} ( {hashIdProp.intValue} )", label.image, label.tooltip );
		var newObj = EditorGUI.ObjectField( position, displayLabel, current, tType, false ) as HashedScriptableObject;
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
		if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) )
			return type.GetGenericArguments()[0];
		return type;
	}

	private static Type GetElementType( Type type )
	{
		while( type != null && type != typeof( object ) )
		{
			if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( BaseHsoRef<> ) )
				return type.GetGenericArguments()[0];
			type = type.BaseType;
		}
		return null;
	}

	private static IHashedSOCollection GetCollection( Type tType )
	{
		if( _collectionCache.TryGetValue( tType, out var cached ) ) return cached;
		foreach( var guid in AssetDatabase.FindAssets( $"t:{tType.Name}" ) )
		{
			var asset = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( guid ), tType ) as HashedScriptableObject;
			var collection = asset?.GetCollection();
			if( collection == null ) continue;
			_collectionCache[tType] = collection;
			return collection;
		}
		return null;
	}
}
