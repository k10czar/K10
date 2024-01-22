using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorScriptableObjectUtils
{
	public static void CreateInsideMenu( string rootAssetPath, SerializedProperty prop, System.Type type, bool focus = false, System.Action<ScriptableObject> OnObjectCreated = null, string name = null )
	{
		System.Type selectedType = null;

		var types = System.AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);

		var count = types.Count();
		if (count <= 1)
		{
			selectedType = type;
			foreach (var t in types) selectedType = t;
			Debug.Log( selectedType.ToStringOrNull() + " is the only non Abstract type that implements " + type + ". So dont need to show menu" );
			
			Debug.Log( $"{rootAssetPath} + {name ?? (prop.PropPathParsed() + " + _ + " + selectedType.ToStringOrNull())}" );
		}
		else
		{
			GenericMenu menu = new GenericMenu();

			foreach (var t in types)
			{
				var pathAtt = t.GetCustomAttribute<CreationPathAttribute>();
				var tParsed = ( pathAtt != null ? pathAtt.Path : t.ToStringOrNull() ).Replace( ".", "/" );

				var objName = name ?? (prop.PropPathParsed() + " + _ + " + t.ToStringOrNull());
				Debug.Log( $"{rootAssetPath} + {objName}" );

				GenericMenu.MenuFunction2 onTypedElementCreatedInside = ( tp ) => ScriptableObjectUtils.CreationObjectInsideAssetFile( (System.Type)tp, rootAssetPath, objName, focus, OnObjectCreated );
				menu.AddItem( new GUIContent( tParsed ), false, onTypedElementCreatedInside, t );
			}

			menu.ShowAsContext();

			Debug.Log(type.ToStringOrNull() + " is a abstract ScriptableObject click again the button holding some of the following keys to choose some of the inherited type:\n" + string.Join("\n", types ) + "\n\n");
			return;
		}

		ScriptableObjectUtils.CreationObjectInsideAssetFile( selectedType, rootAssetPath, name ?? (prop.PropPathParsed() + " + _ + " + selectedType.ToStringOrNull()), focus, OnObjectCreated );
	}

	public static void CreateMenu( string rootAssetPath, SerializedProperty prop, System.Type type, bool focus = false, System.Action<ScriptableObject> OnObjectCreated = null )
	{
		System.Type selectedType = null;

		var types = System.AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);

					

		var count = types.Count();
		if (count <= 1)
		{
			selectedType = type;
			foreach (var t in types) selectedType = t;
			Debug.LogError( selectedType.ToStringOrNull() + " is the only non Abstract type that implements " + type + ". So dont need to show menu" );
		}
		else
		{
			GenericMenu menu = new GenericMenu();

			foreach (var t in types)
			{
				var pathAtt = t.GetCustomAttribute<CreationPathAttribute>();
				var tParsed = ( pathAtt != null ? pathAtt.Path : t.ToStringOrNull() ).Replace( ".", "/" );
				
				GenericMenu.MenuFunction2 onTypedElementCreatedOutside = ( tp ) =>
				{
					var rootFolder = System.IO.Path.GetDirectoryName( rootAssetPath );
					var newFolderName = System.IO.Path.GetFileNameWithoutExtension( rootAssetPath );
					ScriptableObjectUtils.CreationObjectAndFile( (System.Type)tp, rootFolder + "\\" + prop.ToFileName() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				};

				menu.AddItem( new GUIContent( tParsed + "/Separated File" ), false, onTypedElementCreatedOutside, t );

				GenericMenu.MenuFunction2 onTypedElementCreatedOutsideInFolder = ( tp ) =>
				{
					var rootFolder = System.IO.Path.GetDirectoryName( rootAssetPath );
					var newFolderName = System.IO.Path.GetFileNameWithoutExtension( rootAssetPath );
					ScriptableObjectUtils.CreationObjectAndFile( (System.Type)tp, rootFolder + "\\" + newFolderName + "\\" + prop.ToFileName() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				};

				menu.AddItem( new GUIContent( tParsed + "/Separated File in Folder" ), false, onTypedElementCreatedOutsideInFolder, t );

				GenericMenu.MenuFunction2 onTypedElementCreatedInside = ( tp ) => ScriptableObjectUtils.CreationObjectInsideAssetFile( (System.Type)tp, rootAssetPath, prop.PropPathParsed() + "_" + tp.ToStringOrNull(), focus, OnObjectCreated );
				menu.AddItem( new GUIContent( tParsed + "/Nested inside this SO" ), false, onTypedElementCreatedInside, t );
			}

			menu.ShowAsContext();

			Debug.Log(type.ToStringOrNull() + " is a abstract ScriptableObject click again the button holding some of the following keys to choose some of the inherited type:\n" + string.Join("\n", types ) + "\n\n");
			return;
		}

		ScriptableObjectUtils.CreationObjectAndFile( selectedType, rootAssetPath, focus, OnObjectCreated );
	}
}
