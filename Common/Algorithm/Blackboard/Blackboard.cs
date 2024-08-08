using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using K10;

[Serializable]
public class Blackboard : IBlackboard
{
    Dictionary<string, object> entries = new();

    public void Debug( string nameToDebug = null ) 
    {
        var SB = ObjectPool<StringBuilder>.Request();
        SB.AppendLine( $"Blackboard: {(nameToDebug != null ? nameToDebug : "")}" );
        foreach (var entry in entries) {
            var key = entry.Key;
            if( TagsDebug.TryFind( key, out var name ) ) key = $"{name}({key})";
            SB.AppendLine($"[{key}]: {entry.Value.ToStringOrNull()}");
        }
        UnityEngine.Debug.Log( SB.ToString() );
        ObjectPool<StringBuilder>.Return( SB );
    }

    public bool TryGet<T>(string key, out T value) {
        if (entries.TryGetValue(key, out var entry) && entry is T castedEntry) {
            value = castedEntry;
            return true;
        }
        
        value = default;
        return false;
    }
    
    public void Set<T>(string key, T value) {
        entries[key] = value;
    }
    
    public bool ContainsKey(string key) => entries.ContainsKey(key);
    public bool Remove(string key) => entries.Remove(key);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => entries.GetEnumerator();
}