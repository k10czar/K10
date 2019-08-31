using UnityEngine;
using UnityEditor;
using System.Collections;

namespace K10.EditorGUIExtention
{
	public static class ComponentField
	{
		public static void Draw<T>( Rect r, SerializedProperty prop, string newPath ) where T : MonoBehaviour
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var iconSize = 16;
				var create = GUI.Button( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), IconCache.Get( "match" ).Texture, new GUIStyle() );
				if( create )
				{
					var go = K10EditorGUIUtils.CreateSequentialGO<T>( newPath );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
				}
				r.x += iconSize + 2;
				r.width -= iconSize + 2;
			}

			Draw<T>( r, prop, false );
		}

		public static void Draw<T>( Rect r, SerializedProperty prop, bool allowSceneObjects ) where T : MonoBehaviour
		{
			var obj = prop.objectReferenceValue;

			var newObj = EditorGUI.ObjectField( r, obj, typeof( GameObject ), allowSceneObjects );
			if( newObj != obj )
			{
				if( newObj != null )
				{
					var comp = ( (GameObject)newObj ).GetComponent<T>();
					if( comp != null )
					{
						prop.objectReferenceValue = comp;
						prop.serializedObject.ApplyModifiedProperties();
					}
					else EditorUtility.DisplayDialog( "Warning!", "Cannot find (" + typeof( T ).ToString() + ") component on prefab (" + obj.NameOrNull() + ") root transform", "OK" );
				}
				else
				{
					prop.objectReferenceValue = newObj;
					prop.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		public static void Layout<T>( SerializedProperty prop, bool allowSceneObjects ) where T : Behaviour
		{
			var obj = prop.objectReferenceValue;

			var newObj = EditorGUILayout.ObjectField( obj, typeof( GameObject ), allowSceneObjects );
			if( newObj != obj )
			{
				if( newObj != null )
				{
					var comp = ( (GameObject)newObj ).GetComponent<T>();
					if( comp != null )
					{
						prop.objectReferenceValue = comp;
						prop.serializedObject.ApplyModifiedProperties();
					}
					else EditorUtility.DisplayDialog( "Warning!", "Cannot find (" + typeof( T ).ToString() + ") component on prefab (" + obj.name + ") root transform", "OK" );
				}
				else
				{
					prop.objectReferenceValue = newObj;
					prop.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		public static void Layout<T>( SerializedProperty prop, string newPath ) where T : Behaviour
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var create = GUILayout.Button( IconCache.Get( "match" ).Texture, new GUIStyle(), GUILayout.Width( 16 ) );
				if( create )
				{
					var go = K10EditorGUIUtils.CreateSequentialGO<T>( newPath );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
				}
			}

			Layout<T>( prop, false );
		}

		public static void SceneLayout<T>( SerializedProperty prop, string desiredName ) where T : Behaviour
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var create = GUILayout.Button( IconCache.Get( "match" ).Texture, new GUIStyle(), GUILayout.Width( 16 ) );
				if( create )
				{
					var go = K10EditorGUIUtils.CreateSceneGO<T>( desiredName );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
				}
			}

			Layout<T>( prop, false );
		}
	}


	public static class ScriptableObjectField
	{
		public static void Draw<T>( Rect r, SerializedProperty prop, string newPath ) where T : ScriptableObject
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var iconSize = 16;
				var create = GUI.Button( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), IconCache.Get( "match" ).Texture, new GUIStyle() );
				if( create )
				{
					var go = ScriptableObjectUtils.CreateSequential<T>( newPath );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
				}
				r = r.CutLeft( iconSize + 2 );
			}

			Draw<T>( r, prop, false );
		}

		public static void Draw<T>( Rect r, SerializedProperty prop, bool allowSceneObjects ) where T : ScriptableObject
		{
			var obj = prop.objectReferenceValue;

			var newObj = EditorGUI.ObjectField( r, obj, typeof( T ), allowSceneObjects );
			if( newObj != obj )
			{
				prop.objectReferenceValue = newObj;
				prop.serializedObject.ApplyModifiedProperties();
			}
		}

		public static void Layout<T>( SerializedProperty prop, string newPath, params GUILayoutOption[] options ) where T : ScriptableObject
		{
			var obj = prop.objectReferenceValue;

			GUILayout.BeginHorizontal();
			if( obj == null )
			{
				var icon = IconCache.Get( "match" ).Texture;
				var slh = EditorGUIUtility.singleLineHeight;
				var create = GUILayout.Button( icon, new GUIStyle(), GUILayout.Width( ( icon.width / icon.height ) * slh ), GUILayout.Height( slh ) );
				if( create )
				{
					var go = ScriptableObjectUtils.CreateSequential<T>( newPath );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
				}
			}

			Layout<T>( prop, false, options );
			GUILayout.EndHorizontal();
		}

		public static void Layout<T>( SerializedProperty prop, bool allowSceneObjects, params GUILayoutOption[] options ) where T : ScriptableObject
		{
			var obj = prop.objectReferenceValue;
			var newObj = Layout<T>( obj, allowSceneObjects, options );
			if( newObj != obj )
			{
				prop.objectReferenceValue = newObj;
				prop.serializedObject.ApplyModifiedProperties();
			}
		}

		public static T Layout<T>( Object obj, bool allowSceneObjects, params GUILayoutOption[] options ) where T : ScriptableObject
		{
			return (T)EditorGUILayout.ObjectField( obj, typeof( T ), allowSceneObjects, options );
		}
	}
}
