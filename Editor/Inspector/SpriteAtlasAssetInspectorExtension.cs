using UnityEditor;
using UnityEditor.U2D;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[CustomEditor(typeof(SpriteAtlasImporter))]
[CanEditMultipleObjects]
public class SpriteAtlasAssetInspectorExtension : Editor
{
    private Editor _nativeEditor;

    private void OnEnable()
    {
        var t = Type.GetType("UnityEditor.U2D.SpriteAtlasImporterInspector, UnityEditor");
        if (t != null) _nativeEditor = CreateEditor(targets, t);
    }

    private void OnDisable()
    {
        if (_nativeEditor != null) DestroyImmediate(_nativeEditor);
    }

    public override void OnInspectorGUI()
    {
        var assetPath = ((AssetImporter)target).assetPath;
        var packables = GetPackables(assetPath);
        SpriteAtlasTools.DrawTools(packables);
        if (_nativeEditor != null) _nativeEditor.OnInspectorGUI();
        else DrawDefaultInspector();
    }

    // SpriteAtlasImporter is the importer for .spriteatlasv2 files.
    // Packables are stored in the asset YAML under m_ImporterData.packables.
    private static UnityEngine.Object[] GetPackables(string assetPath)
    {
        var fullPath = System.IO.Path.GetFullPath(assetPath);
        if (!System.IO.File.Exists(fullPath)) return Array.Empty<UnityEngine.Object>();

        var lines = System.IO.File.ReadAllLines(fullPath);
        var packables = new List<UnityEngine.Object>();
        bool inPackables = false;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("packables:")) { inPackables = true; continue; }
            if (!inPackables) continue;
            if (!trimmed.StartsWith("- {")) { inPackables = false; continue; }
            var match = Regex.Match(trimmed, @"guid:\s*([a-f0-9]+)");
            if (!match.Success) continue;
            var objPath = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
            if (string.IsNullOrEmpty(objPath)) continue;
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(objPath);
            if (obj != null) packables.Add(obj);
        }
        return packables.ToArray();
    }
}
