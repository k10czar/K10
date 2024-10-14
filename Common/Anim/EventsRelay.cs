using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsRelay : MonoBehaviour
{
    EventSlot empty;
    EventSlot<string> withKey;

    [SerializeReference,ExtendedDrawer] ITriggerable[] onTriggerEmpty;
    [SerializeField] KeyEvent[] onTriggerKeys;

    public IEventRegister Empty => empty ??= new();
    public IEventRegister<string> WithKey => withKey ??= new();
    
    public void Trigger() 
    {
        empty?.Trigger();
        onTriggerEmpty.TriggerAll();
    }
    
    public void TriggerWithKey( string key ) 
    {
        withKey?.Trigger( key );
        foreach( var t in onTriggerKeys ) { t.TryTrigger( key ); }
    }

    [System.Serializable]
    public class KeyEvent
    {
        [SerializeField] string key;
        [SerializeReference,ExtendedDrawer] ITriggerable[] onTrigger;

        public bool TryTrigger(string key)
        {
            var isKeyMatch = string.Equals( key, this.key, StringComparison.Ordinal );
            if( !isKeyMatch ) return false;
            onTrigger.TriggerAll();
            return true;
        }
    }
    
    // public void TriggerWithFloat( float value ) 
    // {
        
    // }
    
    // public void TriggerWithInt( int value ) 
    // {
        
    // }
}
