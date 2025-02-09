using System.Collections;
using System.Text;
using K10;
using UnityEngine;

public class RuntimeTriggersRegister : MonoBehaviour
{
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
		[ExtendedDrawer(true),SerializeReference] ITriggerable _do;
		[ExtendedDrawer(true),SerializeReference] IEventBinderReference _when;

#if UNITY_EDITOR
		public void EDITOR_OnInspectEdit()
		{
			_name = $"Do {_do.TrySummarize()} when {_when.TrySummarize()}";
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
