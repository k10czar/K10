using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

[CustomEditor(typeof(SpriteAtlasImporter))]
[CanEditMultipleObjects]
public class SpriteAtlasAssetInspectorExtension : Editor
{
    private Editor _nativeEditor;
    private int _previewPage;
    private bool _previewFoldout = true;

    private static readonly MethodInfo _getPreviewTextures =
        typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

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
        DrawAtlasPreview(assetPath);
    }

    private void DrawAtlasPreview(string assetPath)
    {
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
        if (atlas == null) return;
        if (_getPreviewTextures == null) return;
        if (_getPreviewTextures.Invoke(null, new object[] { atlas }) is not Texture2D[] pages || pages.Length == 0) return;

        _previewPage = Mathf.Clamp(_previewPage, 0, pages.Length - 1);
        var tex = pages[_previewPage];

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        _previewFoldout = EditorGUILayout.Foldout(_previewFoldout, "Generated Textures", true, EditorStyles.foldoutHeader);
        GUILayout.FlexibleSpace();
        if (tex != null)
            EditorGUILayout.LabelField($"{tex.width} x {tex.height}", GUILayout.Width(90));
        EditorGUILayout.LabelField($"{pages.Length} page{(pages.Length != 1 ? "s" : "")}", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        if (_previewFoldout)
        {
            if (pages.Length > 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_previewPage == 0);
                if (GUILayout.Button("< Prev")) _previewPage--;
                EditorGUI.EndDisabledGroup();
                GUILayout.Label($"Page {_previewPage + 1} / {pages.Length}", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.BeginDisabledGroup(_previewPage == pages.Length - 1);
                if (GUILayout.Button("Next >")) _previewPage++;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            if (tex != null)
            {
                float maxSize = EditorGUIUtility.currentViewWidth - 30f;
                float aspect = (float)tex.height / tex.width;
                float w = Mathf.Min(maxSize, tex.width);
                float h = w * aspect;
                var rect = GUILayoutUtility.GetRect(w, h, GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(rect, tex);
            }
        }

        EditorGUILayout.EndVertical();
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
            if (obj == null) continue;
            if (obj is DefaultAsset)
            {
                foreach (var texGuid in AssetDatabase.FindAssets("t:Texture2D", new[] { objPath }))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(texGuid));
                    if (tex != null) packables.Add(tex);
                }
            }
            else packables.Add(obj);
        }
        return packables.ToArray();
    }
}
