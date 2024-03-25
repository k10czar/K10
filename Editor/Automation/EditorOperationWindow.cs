using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using Automation;


[InitializeOnLoad]
public sealed class AutomationWindow : EditorWindow
{
	private static bool _alreadyTryed = false;

	[MenuItem( "K10/Automation/Config" )] static void Open() { var i = Instance; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void Init()
	{
		_alreadyTryed = false;   
	}

	static AutomationWindow _instance;
	public static AutomationWindow Instance
	{
		get
		{
			if( _instance == null ) _instance = GetWindow<AutomationWindow>( "Automation" );
			return _instance;
		}
	}

    static AutomationWindow()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
			TriggerGameStart();
        }
    }

	private static string _automationData;
	public static string AutomationKey => _automationData ?? ( _automationData = "Automation@" + Application.dataPath );

	public static AutomationData _data;
	public static AutomationData Data => EditorPrefsUtils.GetPersistent( ref _data, AutomationKey );

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
	
	public static void TriggerGameStart( MonoBehaviour mb = null, bool log = true )
	{
		if( _alreadyTryed ) return;
		_alreadyTryed = true;

		var data = Data;
		if( data == null )
		{
			Debug.LogError( "Cannot start Automation operation because persistent data is null" );
			return;
		}
		if( data.operation == null )
		{
			Debug.Log( "No Automation operation setted to start" );
			return;
		}

		if( !data.active ) return;
		data.operation.ExecuteOn( mb, log );
	}

	private void OnGUI()
	{
		var data = Data;
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
			EditorPrefsUtils.SaveData( data, AutomationKey );
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
}