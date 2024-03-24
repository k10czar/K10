using System.Collections.Generic;
using UnityEngine;

namespace Automation
{
	public class EditorOperationRunner : MonoBehaviour
	{
		private static bool _alreadyTryed = false;

#if UNITY_EDITOR
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
				_debugPath = UnityEditor.AssetDatabase.GUIDToAssetPath( _guid );
				operation = UnityEditor.AssetDatabase.LoadAssetAtPath<OperationObject>( _debugPath );
			}

			public void OnBeforeSerialize()
			{
				_debugPath = UnityEditor.AssetDatabase.GetAssetPath( operation );
				_guid = UnityEditor.AssetDatabase.AssetPathToGUID( _debugPath );
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
#endif //UNITY_EDITOR

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Init()
		{
			_alreadyTryed = false;   
		}

		[SerializeField] bool _log = true;

		public void Start()
		{
			TriggerGameStart( this, _log);
		}
	}
}
