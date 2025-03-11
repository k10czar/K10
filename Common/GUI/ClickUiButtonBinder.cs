using UnityEngine;
using UnityEngine.UI;

public class ClickUiButtonBinder : IEventBinderReference, ISummarizable
{
    [SerializeField] Button _button;

    public void Register(IEventTrigger listener)
    {
        if( _button == null ) return;
        _button.onClick.AddListener( listener.Trigger );
    }

    public bool Unregister(IEventTrigger listener)
    {
        if( _button == null ) return false;
        _button.onClick.RemoveListener( listener.Trigger );
        return true;
    }

    public string Summarize()
    {
        if( _button == null ) return "UNSETTED"; 
        return $"🔘{_button.name}:Clicked";
    }
}