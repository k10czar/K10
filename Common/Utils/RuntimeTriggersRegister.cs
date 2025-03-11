using UnityEngine;

public class RuntimeTriggersRegister : MonoBehaviour
{
// #if UNITY_EDITOR
// 	[SerializeField,TextArea] string comment; 
// #endif
	[SerializeField] TriggerRegister[] _actions;

	void Start()
	{
		foreach (var action in _actions) action?.Register();
	}

	void OnDestroy()
	{
		foreach (var action in _actions) action?.Unregister();
	}

	[System.Serializable]
    private class TriggerRegister : ICallOnInspectEdit
    {
#if UNITY_EDITOR
		[SerializeField,HideInInspector] public string _name = "ElementName";
#endif
		[ExtendedDrawer(true),SerializeReference] IEventBinderReference _when;
		[ExtendedDrawer(true),SerializeReference] ITriggerable _do;

#if UNITY_EDITOR
		public void EDITOR_OnInspectEdit()
		{
			_name = $"When {_when.TrySummarize()} do {_do.TrySummarize()}";
		}
#endif

        public void Register()
        {
			if( _when == null ) return;
			if( _do == null ) return;
			_when.Register( _do.Trigger );
        }

        public void Unregister()
        {
			if( _when == null ) return;
			if( _do == null ) return;
			_when.Unregister( _do.Trigger );
        }
    }
}
