using UnityEngine;
using UnityEditor;
using System.Collections;

namespace K10.EditorGUIExtention
{
	public static class ComponentField
	{
		public static void Draw<T>( Rect r, SerializedProperty prop, string newFolderPath ) where T : MonoBehaviour
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var iconSize = 16;
				var create = GUI.Button( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), IconCache.Get( "match" ).Texture, new GUIStyle() );
				if( create )
				{
					var go = K10EditorGUIUtils.CreateSequentialGO<T>( newFolderPath + prop.ToFileName() );
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

		public static void Layout<T>( SerializedProperty prop, string newFolderPath ) where T : Behaviour
		{
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var create = GUILayout.Button( IconCache.Get( "match" ).Texture, new GUIStyle(), GUILayout.Width( 16 ) );
				if( create )
				{
					var go = K10EditorGUIUtils.CreateSequentialGO<T>( newFolderPath + prop.ToFileName() );
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
		const string FALLBACK_CREATION_FOLDER = "Assets/Database/SO/";

		public static Object Draw( Rect r, string label, Object obj, System.Type type, string path = null, bool ignoreIdentation = true, bool focus = false )
		{
			if( obj == null )
			{
				var iconSize = 18;
				var create = IconButton.Draw( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), "match", 'C' );
				if( create )
				{
					var selectedAssetPath = path;
					if( string.IsNullOrEmpty( selectedAssetPath ) ) selectedAssetPath = FALLBACK_CREATION_FOLDER + type.ToString();
					obj = ScriptableObjectUtils.CreationObjectAndFile( type, selectedAssetPath, focus );
				}
				r = r.CutLeft( iconSize + 2 );
			}
			if( ignoreIdentation ) EditorGuiIndentManager.New( 0 );
			obj = EditorGUI.ObjectField( r, label, obj, type, false );
			if( ignoreIdentation ) EditorGuiIndentManager.Revert();

			return obj;
		}

		public static bool Draw( Rect r, SerializedProperty prop, System.Type type, string path = null, bool ignoreIdentation = true )
		{
			var createdNewSO = false;
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var iconSize = 18;
				var create = IconButton.Draw( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), "match", 'C' );
				if( create )
				{
					var selectedAssetPath = path;

					if( selectedAssetPath == null )
					{
						var target = prop.serializedObject.targetObject;
						if( prop.serializedObject.targetObject is MonoBehaviour )
						{
							target = MonoScript.FromMonoBehaviour( (MonoBehaviour)prop.serializedObject.targetObject );
							selectedAssetPath = FALLBACK_CREATION_FOLDER + type.ToString();
						}
						else selectedAssetPath = AssetDatabase.GetAssetPath( target );
					}

					System.Action<ScriptableObject> setRef = (newRef) =>
					{
						// prop.serializedObject.SetIsDifferentCacheDirty();
						// prop.serializedObject.Update();
						prop.objectReferenceValue = newRef;
						prop.serializedObject.ApplyModifiedProperties();
						EditorUtility.SetDirty( prop.serializedObject.targetObject );
					};

					// Debug.Log( $"Debug paths: \n" +
					// 			$"AssetDatabase.GetAssetPath( t ) = {selectedAssetPath}\n" +
					// 			$"GetDirectoryName( aPath ) = {System.IO.Path.GetDirectoryName( selectedAssetPath )}\n" +
					// 			$"GetFullPath( aPath ) = {System.IO.Path.GetFullPath( selectedAssetPath )}\n" +
					// 			$" = {0}" );

					EditorScriptableObjectUtils.CreateMenu( selectedAssetPath, prop, type, false, setRef );
					
					createdNewSO = true;
				}
				r = r.CutLeft( iconSize + 2 );
			}

			if( ignoreIdentation ) EditorGuiIndentManager.New( 0 );
			var newObj = EditorGUI.ObjectField( r, obj, type, false );
			if( ignoreIdentation ) EditorGuiIndentManager.Revert();
			if( newObj != obj )
			{
				prop.objectReferenceValue = newObj;
				prop.serializedObject.ApplyModifiedProperties();
			}

			return createdNewSO;
		}

		public static bool Draw<T>( Rect r, SerializedProperty prop, string newFolderPath ) where T : ScriptableObject
		{
			var createdNewSO = false;
			var obj = prop.objectReferenceValue;

			if( obj == null )
			{
				var iconSize = 16;
				var create = GUI.Button( new Rect( r.x, r.y + ( r.height - iconSize ) / 2, iconSize, iconSize ), IconCache.Get( "match" ).Texture, new GUIStyle() );
				if( create )
				{
					var go = ScriptableObjectUtils.CreateSequential<T>( newFolderPath + prop.ToFileName() );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
					createdNewSO = true;
				}
				r = r.CutLeft( iconSize + 2 );
			}

			Draw<T>( r, prop, false );
			return createdNewSO;
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

		public static void InsideLayout<T>( SerializedProperty prop, string name = null, params GUILayoutOption[] options )
		{
			InsideLayout( prop, typeof(T), name, options );
		}

		public static void InsideLayout( SerializedProperty prop, System.Type type, string name = null, params GUILayoutOption[] options )
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
					var path = AssetDatabase.GetAssetPath( prop.serializedObject.targetObject );
					EditorScriptableObjectUtils.CreateInsideMenu( path, prop, type, false, ( go ) => {
						Debug.Log( $"prop.serializedObject: {prop.serializedObject.ToStringOrNull()} {prop.serializedObject.targetObject.NameOrNull()}" );
						if( prop.serializedObject != null ) Debug.Log( $"prop.serializedObject.targetObject: {prop.serializedObject.targetObject.NameOrNull()}" );
						var obj = prop.serializedObject;
						obj.Update();
						// obj.FindProperty(  ); // Get prop again from obj
						prop.objectReferenceValue = go;
						obj.ApplyModifiedProperties();
					}, name );
				}
			}
			
			var newObj = EditorGUILayout.ObjectField( obj, type, false, options );
			if( newObj != obj )
			{
				prop.objectReferenceValue = newObj;
				prop.serializedObject.ApplyModifiedProperties();
			}

			GUILayout.EndHorizontal();
		}


		public static bool Layout<T>( SerializedProperty prop, string newFolderPath, params GUILayoutOption[] options ) where T : ScriptableObject
		{
			var createdNewSO = false;
			var obj = prop.objectReferenceValue;

			GUILayout.BeginHorizontal();
			if( obj == null )
			{
				var icon = IconCache.Get( "match" ).Texture;
				var slh = EditorGUIUtility.singleLineHeight;
				var create = GUILayout.Button( icon, new GUIStyle(), GUILayout.Width( ( icon.width / icon.height ) * slh ), GUILayout.Height( slh ) );
				if( create )
				{
					var go = ScriptableObjectUtils.CreateSequential<T>( newFolderPath + prop.ToFileName() );
					prop.objectReferenceValue = go;
					prop.serializedObject.ApplyModifiedProperties();
					createdNewSO = true;
				}
			}

			Layout<T>( prop, false, options );
			GUILayout.EndHorizontal();
			return createdNewSO;
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
