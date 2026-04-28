using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteAtlas))]
[CanEditMultipleObjects]
public class SpriteAtlasInspectorExtension : Editor
{
    private Editor _nativeEditor;

    private void OnEnable()
    {
        var t = Type.GetType("UnityEditor.U2D.SpriteAtlasInspector, UnityEditor");
        if (t != null) _nativeEditor = CreateEditor(targets, t);
    }

    private void OnDisable()
    {
        if (_nativeEditor != null) DestroyImmediate(_nativeEditor);
    }

    public override void OnInspectorGUI()
    {
        var atlas = (SpriteAtlas)target;
        SpriteAtlasTools.DrawTools(atlas.GetPackables());
        if (GUILayout.Button("Convert to SpriteAtlas V2"))
            ConvertToV2(atlas);
        if (_nativeEditor != null) _nativeEditor.OnInspectorGUI();
        else DrawDefaultInspector();
    }

    private static void ConvertToV2(SpriteAtlas v1)
    {
        var v1Path = AssetDatabase.GetAssetPath(v1);
        var v2Path = AssetDatabase.GenerateUniqueAssetPath(
            System.IO.Path.ChangeExtension(v1Path, ".spriteatlasv2"));

        var packables = v1.GetPackables();
        var v2 = new SpriteAtlasAsset();
        if (packables.Length > 0) v2.Add(packables);

        AssetDatabase.CreateAsset(v2, v2Path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(v2Path));
        Debug.Log($"[SpriteAtlas] V1 → V2: '{v1Path}' → '{v2Path}' ({packables.Length} packable(s)).");
    }
}

public static class SpriteAtlasTools
{
    public static void DrawTools(UnityEngine.Object[] packables)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Atlas Tools", EditorStyles.boldLabel, GUILayout.Width(80));
        if (GUILayout.Button($"Select {packables.Length} Packable(s) in Project"))
            Selection.objects = packables;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sprite Mesh Type:", GUILayout.Width(110));
        if (GUILayout.Button("FullRect")) SetMeshType(packables, SpriteMeshType.FullRect);
        if (GUILayout.Button("Tight"))    SetMeshType(packables, SpriteMeshType.Tight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filter Mode:", GUILayout.Width(70));
        if (GUILayout.Button("Point"))     SetFilterMode(packables, FilterMode.Point);
        if (GUILayout.Button("Bilinear"))  SetFilterMode(packables, FilterMode.Bilinear);
        if (GUILayout.Button("Trilinear")) SetFilterMode(packables, FilterMode.Trilinear);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("sRGB:", GUILayout.Width(35));
        if (GUILayout.Button("Enable sRGB"))  SetSRGB(packables, true);
        if (GUILayout.Button("Disable sRGB")) SetSRGB(packables, false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Compression:", GUILayout.Width(85));
        if (GUILayout.Button("None"))   SetCompression(packables, TextureImporterCompression.Uncompressed);
        if (GUILayout.Button("Normal")) SetCompression(packables, TextureImporterCompression.Compressed);
        if (GUILayout.Button("HQ"))     SetCompression(packables, TextureImporterCompression.CompressedHQ);
        if (GUILayout.Button("LQ"))     SetCompression(packables, TextureImporterCompression.CompressedLQ);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private static HashSet<string> CollectTexturePaths(UnityEngine.Object[] packables)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var packable in packables)
        {
            if (packable == null) continue;
            var path = AssetDatabase.GetAssetPath(packable);
            if (string.IsNullOrEmpty(path)) continue;
            if (packable is DefaultAsset)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
                    paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            else paths.Add(path);
        }
        return paths;
    }

    private static void SetMeshType(UnityEngine.Object[] packables, SpriteMeshType meshType)
    {
        var paths = CollectTexturePaths(packables);
        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.spriteMeshType == meshType) continue;
                settings.spriteMeshType = meshType;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log($"[SpriteAtlas] SpriteMeshType → {meshType} on {changed}/{paths.Count} textures.");
    }

    private static void SetFilterMode(UnityEngine.Object[] packables, FilterMode filterMode)
    {
        var paths = CollectTexturePaths(packables);
        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.filterMode == filterMode) continue;
                importer.filterMode = filterMode;
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log($"[SpriteAtlas] FilterMode → {filterMode} on {changed}/{paths.Count} textures.");
    }

    private static void SetCompression(UnityEngine.Object[] packables, TextureImporterCompression compression)
    {
        var paths = CollectTexturePaths(packables);
        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureCompression == compression) continue;
                importer.textureCompression = compression;
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log($"[SpriteAtlas] Compression → {compression} on {changed}/{paths.Count} textures.");
    }

    private static void SetSRGB(UnityEngine.Object[] packables, bool srgb)
    {
        var paths = CollectTexturePaths(packables);
        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.sRGBTexture == srgb) continue;
                importer.sRGBTexture = srgb;
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        Debug.Log($"[SpriteAtlas] sRGB → {srgb} on {changed}/{paths.Count} textures.");
    }
}
