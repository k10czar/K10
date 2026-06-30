#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using K10.Common;
using UnityEditor;
using UnityEngine;

using static Colors.Console;

public static class AssetDatabaseUtils
{
    static readonly string OP = "(".Colorfy(Punctuations);
    static readonly string CP = ")".Colorfy(Punctuations);
    static readonly string OSB = "[".Colorfy(Punctuations);
    static readonly string CSB = "]".Colorfy(Punctuations);
    static readonly string OAB = "<".Colorfy(Punctuations);
    static readonly string CAB = ">".Colorfy(Punctuations);
    static readonly string DOT = ".".Colorfy(Punctuations);

    public static void EnsurePathIsValid(string path, bool debug = false)
    {
        if (debug) Debug.Log($"{"RequestPath".Colorfy(Verbs)}( {path.Colorfy(Names)} )");

        var last = -1;
        for (int i = 0; i < path.Length; i++)
        {
            var c = path[i];
            if (c is not ('/' or '\\')) continue;

            var root = (last >= 0) ? path[..last] : "";
            var folder = path.Substring(last + 1, i - (last + 1));

            if (!AssetDatabase.IsValidFolder(path[..i]))
            {
                AssetDatabase.CreateFolder(root, folder);
                Debug.Log($"{"CreateFolder".Colorfy(Verbs)} folder at {root.Colorfy(Names)}");
            }

            last = i;
        }
    }

    public static T FindFirst<T>(Func<T, bool> filter, bool debug = false) where T : ScriptableObject
    {
        var target = typeof(T).Name;
        var assetNames = AssetDatabase.FindAssets($"t:{target}");

        foreach (var guid in assetNames)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var candidate = AssetDatabase.LoadAssetAtPath<T>(path);

            if (!filter(candidate)) continue;

            if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"FindFirst".Colorfy(Verbs)}{OAB}{target.Colorfy(TypeName)}{CAB} at {path.Colorfy(Names)}");
            return candidate;
        }

        if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetFirst".Colorfy(Verbs)}{OAB}{target.Colorfy(TypeName)}{CAB}: {"NOT FOUND".Colorfy(Negation)}");
        return null;
    }

    public static T GetFirst<T>(string name, bool debug = true) where T : ScriptableObject
    {
        string[] assetNames = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}");
        for (int i = 0; i < assetNames.Length; i++)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(assetNames[i]);
            if (!SOpath.AsPathIsFilename(name)) continue;

            var element = AssetDatabase.LoadAssetAtPath<T>(SOpath);
            if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetFirst".Colorfy(Verbs)}{OAB}{typeof(T).Name.Colorfy(TypeName)}{CAB} at {SOpath.Colorfy(Names)}");
            return element;
        }

        if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetFirst".Colorfy(Verbs)}{OAB}{typeof(T).Name.Colorfy(TypeName)}{CAB}: {"NOT FOUND".Colorfy(Negation)}");
        return null;
    }

    public static T[] GetAll<T>(string name, bool debug = true) where T : ScriptableObject
    {
        string[] assetNames = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}");
        var collection = new List<T>();

        for (int i = 0; i < assetNames.Length; i++)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(assetNames[i]);
            var element = AssetDatabase.LoadAssetAtPath<T>(SOpath);
            if (!SOpath.AsPathIsFilename(name)) continue;
            collection.Add(element);
        }

        if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetAll".Colorfy(Verbs)}{OAB}{typeof(T).Name.Colorfy(TypeName)}{CAB} found {collection.Count}:\n{string.Join(", ", collection.ToList().ConvertAll<string>((so) => so.NameOrNull())).Colorfy(Names)}");
        return collection.ToArray();
    }

    public static T[] GetAll<T>(bool debug = true) where T : ScriptableObject
    {
        string[] assetNames = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var collection = new T[assetNames.Length];

        for (int i = 0; i < assetNames.Length; i++)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(assetNames[i]);
            var element = AssetDatabase.LoadAssetAtPath<T>(SOpath);
            collection[i] = element;
        }

        if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetAll".Colorfy(Verbs)}{OAB}{typeof(T).Name.Colorfy(TypeName)}{CAB} found {collection.Length.ToString().Colorfy(Numbers)}:\n{string.Join(", ", collection.ToList().ConvertAll<string>((so) => so.NameOrNull())).Colorfy(Colors.ArcticLime)}");
        return collection;
    }

    public static ScriptableObject[] GetAll(System.Type type, bool debug = true)
    {
        string[] assetNames = AssetDatabase.FindAssets($"t:{type.Name}");
        var collection = new ScriptableObject[assetNames.Length];

        for (int i = 0; i < assetNames.Length; i++)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(assetNames[i]);
            var element = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SOpath);
            collection[i] = element;
        }

        if (debug) Debug.Log($"{"AssetDatabaseUtils".Colorfy(TypeName)}{DOT}{"GetAll".Colorfy(Verbs)}{OP} {type.Name.Colorfy(Keyword)} {CP} found {collection.Length.ToString().Colorfy(Numbers)}:\n   -{string.Join(",\n   -", collection.ToList().ConvertAll<string>((so) => so.NameOrNull()))}");
        return collection;
    }

    public static T LoadOrCreateSO<T>(string path) where T : ScriptableObject
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var config = AssetDatabase.LoadAssetAtPath<T>(path);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, path);
        }

        return config;
    }
}
#endif