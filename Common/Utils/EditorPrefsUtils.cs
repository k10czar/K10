using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class EditorPrefsUtils
{
	public static void SaveData<T>( T t, string key ) { EditorPrefs.SetString( key, JsonUtility.ToJson( t ) ); }
	
	public static T GetPersistent<T>( ref T t, string saveKey ) where T : new()
	{
		if( t == null )
		{
			var dataJson = EditorPrefs.GetString( saveKey );
			if( !string.IsNullOrEmpty( dataJson ) ) t = JsonUtility.FromJson<T>( dataJson );
			if( t == null ) t = new T();
		}
		return t;
	}
}
#endif