using UnityEngine;
using System.Collections;
using UnityEditor;
using K10.EditorGUIExtention;
using System.Collections.Generic;

public class PreviewButton
{
	public static bool Layout( float size, SerializedProperty property )
	{
		if( property.objectReferenceValue == null ) return false;
		if( property.objectReferenceValue is Component ) return Layout( size, (Component)property.objectReferenceValue );
		else if( property.objectReferenceValue is GameObject ) return Layout( size, (Component)property.objectReferenceValue );
		return false;
	}

	public static bool Layout( float size, Component component ) { return Layout( size, component.gameObject ); }

	public static bool Layout( float size, GameObject go )
	{
		if( go != null )
		{
			var t = AssetPreview.GetAssetPreview( go );
			if( IconButton.Layout( size, "search" ) )
				EditorGUIUtility.PingObject( go );
			return t != null;
		}
		return false;
	}

	public static bool Draw( Rect area, SerializedProperty property )
	{
		if( property.objectReferenceValue == null ) return false;
		if( property.objectReferenceValue is Component ) return Draw( area, (Component)property.objectReferenceValue );
		else if( property.objectReferenceValue is GameObject ) return Draw( area, (Component)property.objectReferenceValue );
		return false;
	}

	public static bool Draw( Rect area, Component component ) { return Draw( area, component.gameObject ); }

	public static bool Draw( Rect area, GameObject go )
	{
		if( go != null )
		{
			var t = AssetPreview.GetAssetPreview( go );
			if( IconButton.Draw( area, t ?? IconCache.Get( "search" ).Texture ) )
				EditorGUIUtility.PingObject( go );
			return t != null;
		}
		return false;
	}
}

public static class ComponentListPicker<T> where T : Component
{
	public static void Layout( SerializedProperty property ) { Layout( property, null ); }

	public static void Layout( SerializedProperty property, string newFilePreffix )
	{
		EditorGUILayout.BeginHorizontal();

		bool valid = ( property.objectReferenceValue != null ) && ( property.objectReferenceValue is T );

		var id = -1;
		if( valid ) id = PrefabCache<T>.Cache.IndexOf( property.objectReferenceValue as T );		
		id++;

		var iconSize = 20;

		if( id == 0 && newFilePreffix != null ) 
		{ 
			if( IconButton.Layout( "add", iconSize, '+', "Create new " + typeof(T).ToString() + " prefab", Color.green ) )
			{
				property.objectReferenceValue = K10EditorGUIUtils.CreateSequentialGO<T>( newFilePreffix + typeof(T) );
				PrefabCache<T>.Refresh();
			}
		}
		else PreviewButton.Layout( iconSize, property.objectReferenceValue as T );

		var nid = EditorGUILayout.Popup( id, PrefabCache<T>.NamesWithNone );

		if( nid != id )
		{
			nid--;
			if( nid < 0 ) property.objectReferenceValue = null;
			else property.objectReferenceValue = PrefabCache<T>.Cache[nid];
		}

		if( IconButton.Layout( "refreshButton", iconSize, 'R', "Refresh prefabs loaded", Color.blue ) )
			PrefabCache<T>.Refresh();

		EditorGUILayout.EndHorizontal();
	}

	public static void Draw( Rect area, SerializedProperty property ) { Draw( area, property, null ); }
	public static void Draw( Rect area, SerializedProperty property, string addFolder )
	{
		bool valid = ( property.objectReferenceValue != null ) && ( property.objectReferenceValue is T );

		var id = -1;
		if( valid ) id = PrefabCache<T>.Cache.IndexOf( property.objectReferenceValue as T );		
		id++;

		var iconSize = Mathf.Min( area.height, area.width / 4 );

		var iconArea = area.CutRight( area.width - iconSize ).RequestHeight( iconSize );
		if( id == 0 && addFolder != null ) 
		{ 
			if( IconButton.Draw( iconArea, "add", '+', "Create new " + typeof(T).ToString() + " prefab", Color.green ) )
			{
				property.objectReferenceValue = K10EditorGUIUtils.CreateSequentialGO<T>( addFolder + "/New" + typeof(T).ToString() );
				PrefabCache<T>.Refresh();
			}
		}
		else PreviewButton.Draw( iconArea, property.objectReferenceValue as T );

		area = area.CutLeft( iconSize ).RequestHeight( EditorGUIUtility.singleLineHeight );

		var nid = EditorGUI.Popup( area.CutRight( iconSize ), id, PrefabCache<T>.NamesWithNone );

		if( nid != id )
		{
			nid--;
			if( nid < 0 ) property.objectReferenceValue = null;
			else property.objectReferenceValue = PrefabCache<T>.Cache[nid];
		}

		if( IconButton.Draw( area.CutLeft( area.width - iconSize ).RequestHeight( iconSize ), "refreshButton", 'R', "Refresh prefabs loaded", Color.blue ) )
			PrefabCache<T>.Refresh();
	}
}
