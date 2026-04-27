using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Linq;
using K10.EditorGUIExtention;

public class UiTexturesReporter : EditorWindow
{
    private GameObject _target;
    private bool _includeDisabled = true;
    private Vector2 _scroll;
    private readonly List<TextureEntry> _entries = new List<TextureEntry>();
    private readonly Dictionary<string, long> _atlasMemories = new Dictionary<string, long>();
    private Comparison<TextureEntry> _lastComparison;

    [MenuItem("K10/Reports/UI Textures Reporter")] private static void Init() { GetWindow<UiTexturesReporter>("UI Textures Reporter"); }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        // _target = Selection.activeGameObject;
        // Scan();
    }

    private void Scan()
    {
        _entries.Clear();
        _atlasMemories.Clear();
        if (_target == null) { Repaint(); return; }

        var atlasLookup = BuildAtlasLookup();
        var texDict = new Dictionary<Texture, TextureEntry>();

        foreach (var img in _target.GetComponentsInChildren<Image>(_includeDisabled))
        {
            if (img == null || img.sprite == null) continue;
            var tex = img.sprite.texture;
            if (tex == null) continue;
            if (!texDict.TryGetValue(tex, out var entry))
            {
                var (inAtlas, atlasName) = GetAtlasInfo(img.sprite, atlasLookup);
                entry = new TextureEntry(tex, inAtlas, atlasName);
                texDict[tex] = entry;
            }
            entry.AddUser(img.gameObject);
        }

        foreach (var raw in _target.GetComponentsInChildren<RawImage>(_includeDisabled))
        {
            if (raw == null || raw.texture == null) continue;
            if (!texDict.TryGetValue(raw.texture, out var entry))
            {
                entry = new TextureEntry(raw.texture, false, null);
                texDict[raw.texture] = entry;
            }
            entry.AddUser(raw.gameObject);
        }

        _entries.AddRange(texDict.Values);
        Sort(_lastComparison);

        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
            if (atlas == null) continue;
            _atlasMemories[atlas.name] = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(atlas);
        }

        Repaint();
    }

    private static Dictionary<string, string> BuildAtlasLookup()
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null) continue;
            foreach (var packable in atlas.GetPackables())
            {
                var path = AssetDatabase.GetAssetPath(packable);
                if (string.IsNullOrEmpty(path)) continue;
                if (packable is DefaultAsset)
                {
                    foreach (var sg in AssetDatabase.FindAssets("t:Sprite", new[] { path }))
                        lookup[AssetDatabase.GUIDToAssetPath(sg)] = atlas.name;
                }
                else
                {
                    lookup[path] = atlas.name;
                }
            }
        }
        return lookup;
    }

    private static (bool inAtlas, string atlasName) GetAtlasInfo(Sprite sprite, Dictionary<string, string> atlasLookup)
    {
        var path = AssetDatabase.GetAssetPath(sprite);
        if (!string.IsNullOrEmpty(path) && atlasLookup.TryGetValue(path, out var atlasName))
            return (true, atlasName);
        return (false, null);
    }

    internal static string FormatMemory(long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024) return $"{bytes / (1024f * 1024f * 1024f):N2} GB";
        if (bytes >= 1024 * 1024) return $"{bytes / (1024f * 1024f):N2} MB";
        if (bytes >= 1024) return $"{bytes / 1024f:N1} KB";
        return $"{bytes} B";
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GuiLabelWidthManager.New( 50 );
        var newTarget = (GameObject)EditorGUILayout.ObjectField("Target", _target, typeof(GameObject), true);
        GuiLabelWidthManager.Revert();
        if (newTarget != _target) { _target = newTarget; Scan(); }
        
        if (GUILayout.Button("Scan", GUILayout.Width(50))) Scan();

        if( Selection.activeGameObject != _target && Selection.activeGameObject != null)
        {
            if( GUILayout.Button("Scan on Selection", GUILayout.Width(130)) ) 
            { 
                _target = Selection.activeGameObject; 
                Scan();
            }
            EditorGUILayout.ObjectField(GUIContent.none, Selection.activeGameObject, typeof(GameObject), true, GUILayout.Width(180));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        var includeDisabled = GUILayout.Toggle(_includeDisabled, "Include Disabled", GUILayout.Width(130));
        if (includeDisabled != _includeDisabled) { _includeDisabled = includeDisabled; Scan(); }
        var totalMemory = 0L;
        var countedAtlases = new HashSet<string>();
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (e.InAtlas)
            {
                if (!string.IsNullOrEmpty(e.AtlasName) && countedAtlases.Add(e.AtlasName))
                    if (_atlasMemories.TryGetValue(e.AtlasName, out var atlasMem))
                        totalMemory += atlasMem;
            }
            else
            {
                totalMemory += e.MemorySize;
            }
        }
        EditorGUILayout.LabelField($"{_entries.Count} unique textures  |  {FormatMemory(totalMemory)} total");
        EditorGUILayout.EndHorizontal();

        var numW = GUILayout.Width(30);
        var texW = GUILayout.Width(200);
        var dimW = GUILayout.Width(50);
        var memW = GUILayout.Width(90);
        var potW = GUILayout.Width(40);
        var mipW = GUILayout.Width(40);
        var atlasW = GUILayout.Width(45);
        var atlasNameW = GUILayout.Width(150);
        var usageW = GUILayout.Width(45);

        var height = position.height - (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
        GuiUtils.Scroll.CalculateLinesToDraw(_scroll, height, out var startId, out var lines);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("#", K10GuiStyles.smallRightStyle, numW);
        if (GUILayout.Button("Texture", texW)) ToggleSort(TEX_NAME_SORT);
        if (GUILayout.Button("W", dimW)) ToggleSort(TEX_WIDTH_SORT);
        if (GUILayout.Button("H", dimW)) ToggleSort(TEX_HEIGHT_SORT);
        if (GUILayout.Button("Memory", memW)) ToggleSort(MEM_SIZE_SORT);
        if (GUILayout.Button("PoT", potW)) ToggleSort(POT_SORT);
        if (GUILayout.Button("Mip", mipW)) ToggleSort(MIP_SORT);
        if (GUILayout.Button("Atlas", atlasW)) ToggleSort(ATLAS_SORT);
        if (GUILayout.Button("Atlas Name", atlasNameW)) ToggleSort(ATLAS_NAME_SORT);
        if (GUILayout.Button("Uses", usageW)) ToggleSort(USAGE_SORT);
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        GuiUtils.Scroll.DrawFakeLines(startId);
        for (int i = startId; i < _entries.Count && i < startId + lines; i++)
            _entries[i].Draw(i, numW, texW, dimW, memW, potW, mipW, atlasW, atlasNameW, usageW);
        GuiUtils.Scroll.DrawFakeLines(_entries.Count - (startId + lines));
        EditorGUILayout.EndScrollView();
    }

    private void Sort(Comparison<TextureEntry> comparison)
    {
        if (comparison == null) return;
        _lastComparison = comparison;
        _entries.Sort(comparison);
    }

    private void ToggleSort(Comparison<TextureEntry> comparison)
    {
        comparison.SetOrRevertOn(ref _lastComparison);
        _entries.Sort(_lastComparison);
    }

    public class TextureEntry
    {
        public readonly Texture Texture;
        public readonly bool InAtlas;
        public readonly string AtlasName;
        public readonly long MemorySize;
        public readonly bool IsPowerOfTwo;
        public readonly bool HasMipMap;
        private readonly List<GameObject> _users = new List<GameObject>();

        public int UsageCount => _users.Count;

        public TextureEntry(Texture texture, bool inAtlas, string atlasName)
        {
            Texture = texture;
            InAtlas = inAtlas;
            AtlasName = atlasName ?? string.Empty;
            MemorySize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            IsPowerOfTwo = IsPot(texture.width) && IsPot(texture.height);
            HasMipMap = texture.mipmapCount > 1;
        }

        private static bool IsPot(int v) => v > 0 && (v & (v - 1)) == 0;

        public void AddUser(GameObject go) => _users.Add(go);

        private static readonly Color _rowEven = new Color(0f, 0f, 0f, 0f);
        private static readonly Color _rowOdd = new Color(0f, 0f, 0f, 0.08f);
        // private static readonly Color _rowNonPot = new Color(1f, 0.5f, 0f, 0.15f);

        public void Draw(int index, GUILayoutOption numW, GUILayoutOption texW, GUILayoutOption dimW, GUILayoutOption memW, GUILayoutOption potW, GUILayoutOption mipW, GUILayoutOption atlasW, GUILayoutOption atlasNameW, GUILayoutOption usageW)
        {
            var rect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, index % 2 == 0 ? _rowEven : _rowOdd);
                // if (!IsPowerOfTwo) EditorGUI.DrawRect(rect, _rowNonPot);
            }
            EditorGUILayout.LabelField((index + 1).ToString(), K10GuiStyles.smallRightStyle, numW);
            EditorGUILayout.ObjectField(Texture, typeof(Texture), false, texW);
            EditorGUILayout.LabelField(Texture.width.ToString(), dimW);
            EditorGUILayout.LabelField(Texture.height.ToString(), dimW);
            EditorGUILayout.LabelField(FormatMemory(MemorySize), memW);
            GuiColorManager.New(IsPowerOfTwo ? Color.green : Color.red);
            EditorGUILayout.LabelField(IsPowerOfTwo ? "Yes" : "No", potW);
            GuiColorManager.New(HasMipMap ? Color.yellow : Color.white);
            EditorGUILayout.LabelField(HasMipMap ? "Yes" : "No", mipW);
            GuiColorManager.New(InAtlas ? Color.green : Color.red);
            EditorGUILayout.LabelField(InAtlas ? "Yes" : "No", atlasW);
            GuiColorManager.Revert( 3 );
            EditorGUILayout.LabelField(AtlasName, atlasNameW);
            if (GUILayout.Button(UsageCount.ToString(), usageW))
                Selection.objects = _users.ToArray<UnityEngine.Object>();
            EditorGUILayout.EndHorizontal();
        }
    }

    private static readonly Comparison<TextureEntry> TEX_NAME_SORT = (a, b) => string.Compare(a.Texture != null ? a.Texture.name : null, b.Texture != null ? b.Texture.name : null, StringComparison.Ordinal);
    private static readonly Comparison<TextureEntry> TEX_WIDTH_SORT = (a, b) => (a.Texture != null ? a.Texture.width : 0).CompareTo(b.Texture != null ? b.Texture.width : 0);
    private static readonly Comparison<TextureEntry> TEX_HEIGHT_SORT = (a, b) => (a.Texture != null ? a.Texture.height : 0).CompareTo(b.Texture != null ? b.Texture.height : 0);
    private static readonly Comparison<TextureEntry> MEM_SIZE_SORT = (a, b) => a.MemorySize.CompareTo(b.MemorySize);
    private static readonly Comparison<TextureEntry> POT_SORT = (a, b) => a.IsPowerOfTwo.CompareTo(b.IsPowerOfTwo);
    private static readonly Comparison<TextureEntry> MIP_SORT = (a, b) => a.HasMipMap.CompareTo(b.HasMipMap);
    private static readonly Comparison<TextureEntry> ATLAS_SORT = (a, b) => a.InAtlas.CompareTo(b.InAtlas);
    private static readonly Comparison<TextureEntry> ATLAS_NAME_SORT = (a, b) => string.Compare(a.AtlasName, b.AtlasName, StringComparison.Ordinal);
    private static readonly Comparison<TextureEntry> USAGE_SORT = (a, b) => a.UsageCount.CompareTo(b.UsageCount);
}
