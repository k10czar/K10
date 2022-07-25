// using UnityEditor;
using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;

[UnityEditor.CustomEditor( typeof( K10.Automation.Loop ) )]
public class LoopEditor : UnityEditor.Editor
{
	private SerializedProperty _repetitions;
	private SerializedProperty _actions;

	private ReorderableListCollection _lists;

	void OnEnable()
	{
		_repetitions = serializedObject.FindProperty( "_repetitions" );
		_actions = serializedObject.FindProperty( "_actions" );
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SeparationLine.Horizontal();
		EditorGUILayout.BeginHorizontal();
		UnityEditor.EditorGUILayout.LabelField( target.name, K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		EditorGUILayout.EndHorizontal();
		SeparationLine.Horizontal();

		// var height = GetPropertyHeight(  );
		// EditorGUILayout.GetControlRect( GUILayout.Height(  ) )

		GuiLabelWidthManager.New( 70 );
		EditorGUILayout.PropertyField( _repetitions );
		GuiLabelWidthManager.Revert();

		//TO DO Better lists recursion

		// _lists.Request()

		EditorGUILayout.PropertyField( _actions );


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

	public float GetPropertyHeight( UnityEditor.SerializedProperty property, GUIContent label )
	{
		return UnityEditor.EditorGUI.GetPropertyHeight( property, label, true );
	}

	public void OnGUI( Rect rect, UnityEditor.SerializedProperty property, GUIContent label )
	{
		GuiLabelWidthManager.New( 70 );
		EditorGUI.PropertyField( rect, property.FindPropertyRelative( "_repetitions" ) );
		GuiLabelWidthManager.Revert();
		// EditorGUI.PropertyField( rect, property, label, true );
		GUI.Box( rect, "Test" );
	}
}
