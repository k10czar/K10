using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

using static Colors.Console;
using System.Text;

public static class MonoScriptUtilities
{
    [LazyConst] static string[] _scriptsGuids = null; 
    [LazyConst] static Dictionary<System.Type,MonoScript> _cachedScripts = null;

    static string[] ScriptsGuids => _scriptsGuids ??= AssetDatabase.FindAssets("t:MonoScript");
    
	[ConstLike] static readonly HashSet<string> IGNORED_ASSEMBLIES =  new HashSet<string>{ "mscorlib", "Cinemachine", "Mirror", "RainbowFolders", "ParrelSync", "SimpleWebTransport", "Telepathy", "log4net" };
	[ConstLike] static readonly string[] IGNORED_ASSEMBLIES_SUFFIX =  new string[]{ "Unity", "System", "Mono", "Bee", "I18N", "Autodesk", "kcp2k", "Mirror", "com.unity", "Newtonsoft", "nunit", "ExCSS" };
	[ConstLike] static readonly string[] IGNORED_CLASS_SUFFIX =  new string[]{ "<" };
	

    [UnityEditor.MenuItem("K10/Reports/Reload Domain Leak Detector")]
    private static void EDITOR_Log()
    {
		var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        StringBuilder sb = new StringBuilder();

        var logs = 0;
		foreach (var assembly in assemblies)
		{
			var assName = assembly.GetName().Name;
			if( IGNORED_ASSEMBLIES.Contains( assName ) ) continue;
			bool ignoredSuffix = false;
			foreach (var suffix in IGNORED_ASSEMBLIES_SUFFIX) ignoredSuffix |= assName.StartsWith( suffix, System.StringComparison.OrdinalIgnoreCase );
			if( ignoredSuffix ) continue;
			foreach (var type in assembly.GetTypes())
			{
                var typeName = type.Name;
                bool ignoredClassSuffix = false;
                foreach (var suffix in IGNORED_CLASS_SUFFIX) ignoredClassSuffix |= typeName.StartsWith( suffix, System.StringComparison.OrdinalIgnoreCase );
                if( ignoredClassSuffix ) continue;
				var staticFields = type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
				if( staticFields.Length == 0 ) continue;
                var onlyReadonly = true;
                sb.Clear();
			    foreach (var field in staticFields) 
                {
                    var isConst = field.IsLiteral;
                    var constLike = field.GetCustomAttribute<ConstLikeAttribute>() != null;
                    var isReadonly = isConst || constLike;
                    onlyReadonly &= isReadonly;
                    if( !isReadonly ) sb.AppendLine( $"  -{field.FieldType.Name.Colorfy(TypeName)} {field.Name.Colorfy(Names)}" );
                }
                if( onlyReadonly ) continue;
				var script = type.EditorGetScript();
                var link = typeName.Colorfy(TypeName);
                if( script != null )
                {
                    var path = AssetDatabase.GetAssetPath(script);
                    if( !string.IsNullOrEmpty( path ) ) link = $"<a href=\"{path}\" line=\"2\">{link}</a>";
                }
                logs++;
				Debug.Log( $"{assName.Colorfy(Abstraction)}.{link} has {staticFields.Length.ToStringColored( Numbers )} static fields:\n{sb}" );
			}
		}
        if( logs == 0 ) Debug.Log( $"No {"statics".Colorfy(TypeName)} {"detected".Colorfy(Verbs)} as possible {"Reload Domain".Colorfy(Keyword)} Leak" );
        else Debug.Log( $"{logs.ToStringColored(Numbers)} class(es) with {"static".Colorfy(TypeName)} {"detected".Colorfy(Verbs)} review for a possible {"Reload Domain".Colorfy(Keyword)} Leak" );
    }

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

    public static Texture EditorGetIcon( this System.Type type )
    {
        var script = EditorGetScript( type );
        if( script == null ) return null;
        return EditorGUIUtility.GetIconForObject( script );
    }
}
