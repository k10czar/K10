using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using Automation;


public sealed class AutomationWindow : EditorWindow
{
	[MenuItem( "K10/Automation/Config" )] static void Init() { var i = Instance; }

	static AutomationWindow _instance;
	public static AutomationWindow Instance
	{
		get
		{
			if( _instance == null ) _instance = GetWindow<AutomationWindow>( "Automation" );
			return _instance;
		}
	}

	private void OnGUI()
	{
		var data = EditorOperationRunner.Data;
		var active = data.active;
		var newActice = active;
		EditorGUILayout.BeginHorizontal();
		if( !active ) GuiColorManager.New( Color.grey );
		if( IconButton.Layout( "robot2", 64f, 'A', active ? "Unactivate" : "Activate", Color.yellow ) ) newActice = !active;
		EditorGUILayout.BeginVertical();
		SeparationLine.Horizontal();
		EditorGUILayout.BeginHorizontal();
		UnityEditor.EditorGUILayout.LabelField( "Automation Operation", K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		EditorGUILayout.EndHorizontal();
		SeparationLine.Horizontal();
		EditorGUILayout.BeginHorizontal();
		GuiLabelWidthManager.New( 126 );
		newActice = UnityEditor.EditorGUILayout.Toggle( "automatic run on play", newActice, GUILayout.Width( 142 ) );
		GuiLabelWidthManager.Revert();
		// UnityEditor.EditorGUILayout.LabelField( "automatic run on play", GUILayout.Width( 126 ) );
		var op = data.operation;
		var newOp = ScriptableObjectField.Layout<OperationObject>( op, false );
		if( op != newOp || active != newActice )
		{
			data.operation = newOp;
			data.active = newActice;
			EditorPrefsUtils.SaveData( data, EditorOperationRunner.AutomationKey );
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		SeparationLine.Horizontal();

		GuiColorManager.New( Color.grey );
		UnityEditor.EditorGUILayout.LabelField( "//TODO: Show selected Loop inspector inline here?" );
		GuiColorManager.Revert();
		// UnityEditor.EditorGUILayout.LabelField( AutomationKey );
		// UnityEditor.EditorGUILayout.TextArea( JsonUtility.ToJson( _data ) );
		// UnityEditor.EditorGUILayout.TextArea( EditorPrefs.GetString( AutomationKey ) );
		if( !active ) GuiColorManager.Revert();
	}

	[System.Serializable]
	public class AutomationData : ISerializationCallbackReceiver
	{
		[SerializeField] public OperationObject operation;
		[SerializeField] public bool active;
		[SerializeField] string _debugPath;
		[SerializeField] string _guid;

		public void OnAfterDeserialize()
		{
			_debugPath = AssetDatabase.GUIDToAssetPath( _guid );
			operation = AssetDatabase.LoadAssetAtPath<OperationObject>( _debugPath );
		}

		public void OnBeforeSerialize()
		{
			_debugPath = AssetDatabase.GetAssetPath( operation );
			_guid = AssetDatabase.AssetPathToGUID( _debugPath );
		}
	}
}