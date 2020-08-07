using System.Collections.Generic;
using UnityEngine;

public class EditorLog : ScriptableObject
{
	const string DEFAULT_DIRECTORY = "Assets/[_Logs_]/";

	[SerializeField] bool _alsoLogToConsole = true;
#if UNITY_EDITOR
	[SerializeField] List<Message> _log = new List<Message>();
#endif

	public void Add( string message )
	{
		var date = System.DateTime.Now;
		var line = new Message( message );
#if UNITY_EDITOR
		_log.Add( line );
#endif
		if( _alsoLogToConsole ) Debug.Log( this.NameOrNull() + " logged new Line\n" + line );
		UnityEditor.EditorUtility.SetDirty( this );
	}

	public static EditorLog CreateNewOne( string name, bool editorOnly = false )
	{
		var dir = DEFAULT_DIRECTORY + ( editorOnly ? "Editor/" : string.Empty );
		var path = dir + name + "Log";
		var asset = ScriptableObject.CreateInstance<EditorLog>();

		asset.Add( $"Created log of {name}" );
		FileAdapter.RequestDirectory( dir );

		string assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath( path + ".asset" );

		UnityEditor.AssetDatabase.CreateAsset( asset, assetPathAndName );

		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();

		return asset;
	}

	[System.Serializable]
	public struct Message
	{
		[SerializeField] long _dateTime;
		[SerializeField] string _author;
		[SerializeField] string _message;

		public Message( string message )
		{
			_dateTime = System.DateTime.Now.ToFileTimeUtc();
			_author = SystemInfo.deviceName;
			_message = message;
		}

		public override string ToString()
		{
			var date = System.DateTime.FromFileTimeUtc( _dateTime );
			return $"{date.ToShortDateString()}\n{date.ToShortTimeString()}\n{_author}\n{_message}";
		}
	}
}
