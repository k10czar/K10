using System.Collections.Generic;
using UnityEditor;

public static class MonoScriptUtilities
{
    static string[] _scriptsGuids = null;
    static Dictionary<System.Type,MonoScript> _cachedScripts = null;

    static string[] ScriptsGuids => _scriptsGuids ??= AssetDatabase.FindAssets("t:MonoScript");

    public static MonoScript EditorGetScript( this System.Type type )
    {
        if( _cachedScripts == null ) _cachedScripts = new();
        if(_cachedScripts.TryGetValue( type, out var script ) ) return script;

        foreach (string guid in ScriptsGuids)
        {
            // Get the asset path from the GUID
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            
            if( monoScript != null && monoScript.GetClass() == type ) 
            {
                _cachedScripts[type] = monoScript;
                return monoScript;
            }
        }

        _cachedScripts[type] = null;
        return null;
    }
}
