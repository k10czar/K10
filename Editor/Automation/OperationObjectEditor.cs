using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using K10.Automation;

[CustomEditor( typeof( OperationObject ) )]
public class OperationObjectEditor : Editor
{
	private SerializedProperty _operation;

	// private ReorderableListCollection _lists;

	void OnEnable()
	{
		_operation = serializedObject.FindProperty( "_operation" );
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SeparationLine.Horizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( target.name, K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		if( IconButton.Layout( "playButton", 24, 'E', "Play loop", Color.blue ) ) ( target as OperationObject ).ExecuteOn();
		EditorGUILayout.EndHorizontal();
		SeparationLine.Horizontal();

		// var height = GetPropertyHeight(  );
		// EditorGUILayout.GetControlRect( GUILayout.Height(  ) )

		//TO DO Better lists recursion

		// _lists.Request()

		EditorGUILayout.PropertyField( _operation );


		// EditorGUILayout.PropertyField( _alsoLogToConsole );

		// SeparationLine.Horizontal();
		// EditorGUILayout.LabelField( "Custom log", K10GuiStyles.boldCenterStyle, GUILayout.Height( 28 ) );
		// EditorGUILayout.BeginHorizontal();
		// GUILayout.TextField( SystemInfo.deviceName );
		// GUILayout.TextField( System.DateTime.Now.ToStringOrNull() );
		// EditorGUILayout.EndHorizontal();
		// textToAdd = GUILayout.TextArea( textToAdd );
		// if( GUILayout.Button( "Add Custom Log" ) )
		// {
		// 	var log = (EditorLog)target;
		// 	log.Add( textToAdd );
		// }
		// SeparationLine.Horizontal();

		// for( int i = _log.arraySize - 1; i >= 0; i-- )
		// {
		// 	var line = _log.GetArrayElementAtIndex( i );
		// 	var dateTime = line.FindPropertyRelative( "_dateTime" );
		// 	var date = System.DateTime.FromFileTimeUtc( dateTime.longValue );
		// 	var author = line.FindPropertyRelative( "_author" );
		// 	var message = line.FindPropertyRelative( "_message" ).stringValue;

		// 	EditorGUILayout.BeginHorizontal();
		// 	GUILayout.TextField( author.stringValue );
		// 	GUILayout.TextField( date.ToStringOrNull() );
		// 	if( IconButton.Layout( "minus", '-', "Remove log line", K10GuiStyles.CYAN_TINT_COLOR ) ) _log.DeleteArrayElementAtIndex( i );
		// 	EditorGUILayout.EndHorizontal();
		// 	GUILayout.TextArea( message );
		// }

		serializedObject.ApplyModifiedProperties();

		// GetPropertyHeight(  )
	}
}
