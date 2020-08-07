using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( EditorLog ) )]
public class EditorLogEditor : Editor
{
	private SerializedProperty _log;
	private SerializedProperty _alsoLogToConsole;
	string textToAdd = "Write a custom log line...";


	void OnEnable()
	{
		_log = serializedObject.FindProperty( "_log" );
		_alsoLogToConsole = serializedObject.FindProperty( "_alsoLogToConsole" );
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SeparationLine.Horizontal();
		EditorGUILayout.LabelField( target.name, K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		SeparationLine.Horizontal();

		EditorGUILayout.PropertyField( _alsoLogToConsole );

		SeparationLine.Horizontal();
		EditorGUILayout.LabelField( "Custom log", K10GuiStyles.boldCenterStyle, GUILayout.Height( 28 ) );
		EditorGUILayout.BeginHorizontal();
		GUILayout.TextField( SystemInfo.deviceName );
		GUILayout.TextField( System.DateTime.Now.ToStringOrNull() );
		EditorGUILayout.EndHorizontal();
		textToAdd = GUILayout.TextArea( textToAdd );
		if( GUILayout.Button( "Add Custom Log" ) ) 
		{
			var log = (EditorLog)target;
			log.Add( textToAdd );
		}
		SeparationLine.Horizontal();

		for( int i = _log.arraySize - 1; i >= 0; i-- )
		{
			var line = _log.GetArrayElementAtIndex( i );
			var dateTime = line.FindPropertyRelative( "_dateTime" );
			var date = System.DateTime.FromFileTimeUtc( dateTime.longValue );
			var author = line.FindPropertyRelative( "_author" );
			var message = line.FindPropertyRelative( "_message" ).stringValue;

			EditorGUILayout.BeginHorizontal();
			GUILayout.TextField( author.stringValue );
			GUILayout.TextField( date.ToStringOrNull() );
			if( IconButton.Layout( "minus", '-', "Remove log line", K10GuiStyles.CYAN_TINT_COLOR ) ) _log.DeleteArrayElementAtIndex(i);
			EditorGUILayout.EndHorizontal();
			GUILayout.TextArea( message );
		}

		serializedObject.ApplyModifiedProperties();
	}
}