using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[CustomEditor(typeof(TextureImporter))]
[CanEditMultipleObjects]
public class TextureAssetInspectorExtension : Editor
{
    private Editor _nativeEditor;
    private List<UnityEngine.Object> _referencingAtlases;
    private bool _textureSearched;

    private class AtlasData
    {
        public UnityEngine.Object Atlas;
        public readonly HashSet<string> DirectPaths = new(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> FolderPaths = new();

        public bool Contains(string texPath)
        {
            if (DirectPaths.Contains(texPath)) return true;
            foreach (var folder in FolderPaths)
                if (texPath.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }

    private static List<AtlasData> _atlasCache;

    private void OnEnable()
    {
        var t = Type.GetType("UnityEditor.TextureImporterInspector, UnityEditor");
        if (t != null) _nativeEditor = CreateEditor(targets, t);
        _textureSearched = false;
    }

    private void OnDisable()
    {
        if (_nativeEditor != null) DestroyImmediate(_nativeEditor);
    }

    public override void OnInspectorGUI()
    {
        if (!_textureSearched) FindReferencingAtlases();
        DrawAtlasReferences();
        serializedObject.Update();
        if (_nativeEditor != null) _nativeEditor.OnInspectorGUI();
        else DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    private static void BuildAtlasCache()
    {
        _atlasCache = new List<AtlasData>();

        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null) continue;
            var data = new AtlasData { Atlas = atlas };
            foreach (var packable in atlas.GetPackables())
            {
                if (packable == null) continue;
                var packPath = AssetDatabase.GetAssetPath(packable);
                if (string.IsNullOrEmpty(packPath)) continue;
                if (packable is DefaultAsset) data.FolderPaths.Add(packPath);
                else data.DirectPaths.Add(packPath);
            }
            _atlasCache.Add(data);
        }

        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlasAsset"))
        {
            var data = BuildV2AtlasData(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null) _atlasCache.Add(data);
        }
    }

    private static AtlasData BuildV2AtlasData(string atlasPath)
    {
        var fullPath = System.IO.Path.GetFullPath(atlasPath);
        if (!System.IO.File.Exists(fullPath)) return null;
        var atlasAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlasAsset>(atlasPath);
        if (atlasAsset == null) return null;

        var data = new AtlasData { Atlas = atlasAsset };
        bool inPackables = false;
        foreach (var line in System.IO.File.ReadAllLines(fullPath))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("packables:")) { inPackables = true; continue; }
            if (!inPackables) continue;
            if (!trimmed.StartsWith("- {")) { inPackables = false; continue; }
            var match = Regex.Match(trimmed, @"guid:\s*([a-f0-9]+)");
            if (!match.Success) continue;
            var packablePath = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
            if (string.IsNullOrEmpty(packablePath)) continue;
            if (AssetDatabase.IsValidFolder(packablePath)) data.FolderPaths.Add(packablePath);
            else data.DirectPaths.Add(packablePath);
        }
        return data;
    }

    private void FindReferencingAtlases()
    {
        if (_atlasCache == null) BuildAtlasCache();
        _textureSearched = true;
        _referencingAtlases = new List<UnityEngine.Object>();
        var texPath = ((AssetImporter)target).assetPath;
        foreach (var data in _atlasCache)
            if (data.Contains(texPath)) _referencingAtlases.Add(data.Atlas);
    }

    private void DrawAtlasReferences()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        var count = _referencingAtlases?.Count ?? 0;
        EditorGUILayout.BeginHorizontal();
        var refreshIcon = EditorGUIUtility.IconContent("Refresh");
        refreshIcon.text = " ▾";
        if (GUILayout.Button(refreshIcon, GUILayout.ExpandWidth(false)))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Full Refresh"), false, () => { BuildAtlasCache(); FindReferencingAtlases(); Repaint(); });
            menu.AddItem(new GUIContent("Recache Project Atlas"), false, () => { BuildAtlasCache(); Repaint(); });
            menu.AddItem(new GUIContent("Find only Texture References"), false, () => { FindReferencingAtlases(); Repaint(); });
            menu.ShowAsContext();
        }
        if( count == 0 ) EditorGUILayout.LabelField($"This texture is NOT used by any Atlas", EditorStyles.boldLabel);
        else EditorGUILayout.LabelField($"🖼 in {count} SpriteAtlas", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        if (count > 0)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                foreach (var atlas in _referencingAtlases)
                    EditorGUILayout.ObjectField(atlas, atlas.GetType(), false);
            }
        }
        EditorGUILayout.EndVertical();
    }
}
