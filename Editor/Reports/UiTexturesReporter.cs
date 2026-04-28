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
    private bool _includeDisabled = true;
    private bool _showTargets = false;
    private Vector2 _scroll;
    private readonly List<GameObject> _targets = new List<GameObject>();
    private readonly List<ContextData> _contexts = new List<ContextData>();
    private readonly Dictionary<string, long> _atlasMemories = new Dictionary<string, long>();
    private Dictionary<string, string> _atlasLookupCache;
    private Comparison<TextureEntry> _lastComparison;
    private readonly List<(Comparison<TextureEntry> base_, bool descending)> _sortStack = new List<(Comparison<TextureEntry>, bool)>();

    [MenuItem("K10/Reports/UI Textures Reporter")] private static void Init() { GetWindow<UiTexturesReporter>("UI Textures Reporter"); }

    private void OnEnable() { Selection.selectionChanged += Repaint; }
    private void OnDisable() { Selection.selectionChanged -= Repaint; }

    private void ScanAll()
    {
        _atlasLookupCache = BuildAtlasLookup();
        _atlasMemories.Clear();
        foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
            if (atlas == null) continue;
            _atlasMemories[atlas.name] = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(atlas);
        }

        _contexts.Clear();
        foreach (var t in _targets)
            _contexts.Add( new(t) );

        foreach (var ctx in _contexts)
            ScanContext(ctx);
        RebuildSharedCounts();
        Repaint();
    }

    private void ScanContext(ContextData ctx)
    {
        ctx.Entries.Clear();
        if (ctx.Target == null) return;
        var lookup = _atlasLookupCache ?? BuildAtlasLookup();
        var texDict = new Dictionary<Texture, TextureEntry>();
        foreach (var img in ctx.Target.GetComponentsInChildren<Image>(_includeDisabled))
        {
            if (img == null || img.sprite == null) continue;
            var tex = img.sprite.texture;
            if (tex == null) continue;
            if (!texDict.TryGetValue(tex, out var entry))
            {
                var (inAtlas, atlasName) = GetAtlasInfo(img.sprite, lookup);
                entry = new TextureEntry(tex, inAtlas, atlasName);
                texDict[tex] = entry;
            }
            entry.AddUser(img.gameObject);
        }
        foreach (var raw in ctx.Target.GetComponentsInChildren<RawImage>(_includeDisabled))
        {
            if (raw == null || raw.texture == null) continue;
            if (!texDict.TryGetValue(raw.texture, out var entry))
            {
                entry = new TextureEntry(raw.texture, false, null);
                texDict[raw.texture] = entry;
            }
            entry.AddUser(raw.gameObject);
        }
        ctx.Entries.AddRange(texDict.Values);
        if (_lastComparison != null) ctx.Entries.Sort(_lastComparison);
    }

    private void RebuildSharedCounts()
    {
        var sharedCount = new Dictionary<Texture, int>();
        foreach (var ctx in _contexts)
        {   
            foreach (var entry in ctx.Entries)
            {
                sharedCount.TryGetValue(entry.Texture, out var c);
                sharedCount[entry.Texture] = c + 1;
            }
        }
        foreach (var ctx in _contexts)
            foreach (var entry in ctx.Entries)
                entry.SharedContextCount = sharedCount.TryGetValue(entry.Texture, out var count) ? count : 1;
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
                else lookup[path] = atlas.name;
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

    private long ComputeTotalMemory(List<TextureEntry> entries)
    {
        var totalMemory = 0L;
        var countedAtlases = new HashSet<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e.InAtlas)
            {
                if (!string.IsNullOrEmpty(e.AtlasName) && countedAtlases.Add(e.AtlasName))
                    if (_atlasMemories.TryGetValue(e.AtlasName, out var atlasMem))
                        totalMemory += atlasMem;
            }
            else totalMemory += e.MemorySize;
        }
        return totalMemory;
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        var includeDisabled = GUILayout.Toggle(_includeDisabled, "Include Disabled", GUILayout.Width(130));
        if (includeDisabled != _includeDisabled) { _includeDisabled = includeDisabled; ScanAll(); }
        if (GUILayout.Button("Scan All", GUILayout.Width(70))) ScanAll();
        if (GUILayout.Button($"+{Selection.gameObjects.Length} Context(s)", GUILayout.Width(100)))
        {
            foreach( var targ in Selection.gameObjects )
            {
                if( _targets.Contains( targ ) ) continue;
                _targets.Add( targ );
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _showTargets = EditorGUILayout.BeginFoldoutHeaderGroup( _showTargets, $"Targets ({_targets.Count})" );
        if (_targets.Count > 0 && GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.Width(28), GUILayout.Height(18)))
        {
            _targets.Clear();
            _contexts.Clear();
            Repaint();
        }
        EditorGUILayout.EndHorizontal();
        if ( _showTargets )
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            int removeIdx = -1, swapA = -1, swapB = -1;
            for (int i = 0; i < _targets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = i > 0;
                if (GUILayout.Button("▲", GUILayout.Width(22))) { swapA = i - 1; swapB = i; }
                GUI.enabled = i < _targets.Count - 1;
                if (GUILayout.Button("▼", GUILayout.Width(22))) { swapA = i; swapB = i + 1; }
                GUI.enabled = true;
                var newGo = (GameObject)EditorGUILayout.ObjectField(_targets[i], typeof(GameObject), true);
                if (newGo != _targets[i]) { _targets[i] = newGo; ScanAll(); }
                if (GUILayout.Button("✕", GUILayout.Width(22))) removeIdx = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx >= 0) { _targets.RemoveAt(removeIdx); ScanAll(); }
            else if (swapA >= 0) { (_targets[swapA], _targets[swapB]) = (_targets[swapB], _targets[swapA]); ScanAll(); }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        var numW = GUILayout.Width(30);
        var texW = GUILayout.Width(250);
        var dimW = GUILayout.Width(50);
        var memW = GUILayout.Width(90);
        var potW = GUILayout.Width(40);
        var mipW = GUILayout.Width(40);
        var atlasW = GUILayout.Width(45);
        var atlasNameW = GUILayout.Width(150);
        var usageW = GUILayout.Width(45);
        var sharedW = GUILayout.Width(60);
        var folderW = GUILayout.Width(250);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("#", K10GuiStyles.smallRightStyle, numW);
        if (GUILayout.Button("Texture", texW)) ToggleSortAll(TEX_NAME_SORT);
        if (GUILayout.Button("W", dimW)) ToggleSortAll(TEX_WIDTH_SORT);
        if (GUILayout.Button("H", dimW)) ToggleSortAll(TEX_HEIGHT_SORT);
        if (GUILayout.Button("Memory", memW)) ToggleSortAll(MEM_SIZE_SORT);
        if (GUILayout.Button("PoT", potW)) ToggleSortAll(POT_SORT);
        if (GUILayout.Button("Mip", mipW)) ToggleSortAll(MIP_SORT);
        if (GUILayout.Button("Atlas", atlasW)) ToggleSortAll(ATLAS_SORT);
        if (GUILayout.Button("Atlas Name", atlasNameW)) ToggleSortAll(ATLAS_NAME_SORT);
        if (GUILayout.Button("Uses", usageW)) ToggleSortAll(USAGE_SORT);
        if (GUILayout.Button("Shared", sharedW)) ToggleSortAll(SHARED_SORT);
        if (GUILayout.Button("Folder")) ToggleSortAll(FOLDER_SORT);
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        int removeIndex = -1;
        for (int ci = 0; ci < _contexts.Count; ci++)
        {
            var ctx = _contexts[ci];
            if (DrawContextHeader(ctx)) { removeIndex = ci; break; }
            if (!ctx.Foldout) continue;
            var selCount = ctx.SelectedCount;
            if (selCount > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{selCount} selected", GUILayout.ExpandWidth(false));
                if (GUILayout.Button($"Create Atlas '{(ctx.Target != null ? ctx.Target.name : "")}' @ {FindCommonFolder(ctx)}", GUILayout.ExpandWidth(false)))
                    CreateAtlas(ctx);
                EditorGUILayout.EndHorizontal();
            }
            for (int i = 0; i < ctx.Entries.Count; i++)
            {
                var (clicked, shift) = ctx.Entries[i].Draw(i, numW, texW, dimW, memW, potW, mipW, atlasW, atlasNameW, usageW, sharedW, folderW);
                if (!clicked) continue;
                if (shift && ctx.LastSelectedIndex >= 0)
                {
                    int from = Math.Min(ctx.LastSelectedIndex, i);
                    int to = Math.Max(ctx.LastSelectedIndex, i);
                    for (int j = from; j <= to; j++) ctx.Entries[j].IsSelected = true;
                }
                else
                {
                    ctx.Entries[i].IsSelected = !ctx.Entries[i].IsSelected;
                }
                ctx.LastSelectedIndex = i;
                Repaint();
            }
        }
        EditorGUILayout.EndScrollView();

        if (removeIndex >= 0)
        {
            _contexts.RemoveAt(removeIndex);
            RebuildSharedCounts();
            Repaint();
        }
    }

    private static readonly Color _ctxHeaderBg = new Color(0.2f, 0.3f, 0.5f, 0.4f);

    private bool DrawContextHeader(ContextData ctx)
    {
        var rect = EditorGUILayout.BeginHorizontal();
        if (Event.current.type == EventType.Repaint) EditorGUI.DrawRect(rect, _ctxHeaderBg);
        var selCount = ctx.SelectedCount;
        var selLabel = selCount > 0 ? $"  [{selCount} selected]" : "";
        ctx.Foldout = EditorGUILayout.Foldout(ctx.Foldout, $"{ctx.Entries.Count} tex  |  {FormatMemory(ComputeTotalMemory(ctx.Entries))}  |  {ctx.Target.NameOrNull()}{selLabel}");
        GuiLabelWidthManager.New(50);
        var newTarget = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, ctx.Target, typeof(GameObject), true);
        GuiLabelWidthManager.Revert();
        if (newTarget != ctx.Target)
        {
            ctx.Target = newTarget;
            ScanContext(ctx);
            RebuildSharedCounts();
            Repaint();
        }
        if (GUILayout.Button("Rescan", GUILayout.Width(55))) { ScanContext(ctx); RebuildSharedCounts(); Repaint(); }
        var removed = GUILayout.Button("✕", GUILayout.Width(22));
        // GuiUtils.Label.ExactSizeLayout($"{ctx.Entries.Count} tex  |  {FormatMemory(ComputeTotalMemory(ctx.Entries))}");
        EditorGUILayout.EndHorizontal();
        return removed;
    }

    private void ToggleSortAll(Comparison<TextureEntry> comparison)
    {
        int idx = _sortStack.FindIndex(k => k.base_ == comparison);
        if (idx == 0)
        {
            _sortStack[0] = (comparison, !_sortStack[0].descending);
        }
        else
        {
            if (idx > 0) _sortStack.RemoveAt(idx);
            _sortStack.Insert(0, (comparison, false));
        }

        var keys = _sortStack.ToArray();
        _lastComparison = (a, b) => {
            for (int i = 0; i < keys.Length; i++)
            {
                var r = keys[i].descending ? keys[i].base_(b, a) : keys[i].base_(a, b);
                if (r != 0) return r;
            }
            return 0;
        };
        foreach (var ctx in _contexts) ctx.Entries.Sort(_lastComparison);
    }

    public class ContextData
    {
        public GameObject Target;
        public bool Foldout = true;
        public int LastSelectedIndex = -1;
        public readonly List<TextureEntry> Entries = new List<TextureEntry>();

        public int SelectedCount => Entries.Count(e => e.IsSelected);

        public ContextData(GameObject target)
        {
            Target = target;
        }
    }

    public class TextureEntry
    {
        public readonly Texture Texture;
        public readonly bool InAtlas;
        public readonly string AtlasName;
        public readonly long MemorySize;
        public readonly bool IsPowerOfTwo;
        public readonly bool HasMipMap;
        public int SharedContextCount = 1;
        public bool IsSelected;
        public readonly string FolderPath;
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
            var assetPath = AssetDatabase.GetAssetPath(texture);
            FolderPath = string.IsNullOrEmpty(assetPath) ? string.Empty : System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? string.Empty;
        }

        private static bool IsPot(int v) => v > 0 && (v & (v - 1)) == 0;

        public void AddUser(GameObject go) => _users.Add(go);

        private static readonly Color _rowEven = new Color(0f, 0f, 0f, 0f);
        private static readonly Color _rowOdd = new Color(0f, 0f, 0f, 0.08f);
        private static readonly Color _rowSelected = new Color(0.2f, 0.5f, 0.9f, 0.25f);

        public (bool clicked, bool shift) Draw(int index, GUILayoutOption numW, GUILayoutOption texW, GUILayoutOption dimW, GUILayoutOption memW, GUILayoutOption potW, GUILayoutOption mipW, GUILayoutOption atlasW, GUILayoutOption atlasNameW, GUILayoutOption usageW, GUILayoutOption sharedW, GUILayoutOption folderW)
        {
            bool clicked = false;
            bool shift = false;
            
            var rect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, index % 2 == 0 ? _rowEven : _rowOdd);
                if (IsSelected) EditorGUI.DrawRect(rect, _rowSelected);
            }
            if (GUILayout.Button((index + 1).ToString(), K10GuiStyles.smallRightStyle, numW))
            {
                clicked = true;
                shift = Event.current.shift;
            }
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
            GuiColorManager.Revert(3);
            EditorGUILayout.LabelField(AtlasName, atlasNameW);
            if (GUILayout.Button(UsageCount.ToString(), usageW))
                Selection.objects = _users.ToArray<UnityEngine.Object>();
            var isExclusive = SharedContextCount <= 1;
            GuiColorManager.New(isExclusive ? Color.green : Color.yellow);
            EditorGUILayout.LabelField(isExclusive ? "Exclusive" : $"{SharedContextCount} ctx", sharedW);
            GuiColorManager.Revert();
            EditorGUILayout.LabelField(FolderPath);
            EditorGUILayout.EndHorizontal();
            return (clicked, shift);
        }
    }

    private static void CreateAtlas(ContextData ctx)
    {
        var selected = ctx.Entries.Where(e => e.IsSelected).ToList();
        if (selected.Count == 0) return;

        var assetPaths = selected
            .Select(e => AssetDatabase.GetAssetPath(e.Texture))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var folder = FindCommonFolder(assetPaths);
        var atlasName = ctx.Target != null ? ctx.Target.name : "NewAtlas";
        var atlasPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{atlasName}.spriteatlas");

        var atlas = new SpriteAtlas();
        var packables = assetPaths
            .Select(p => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p))
            .Where(o => o != null)
            .ToArray();
        atlas.Add(packables);

        AssetDatabase.CreateAsset(atlas, atlasPath);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(atlas);
    }

    private static string FindCommonFolder( ContextData ctx )
    {
        var selected = ctx.Entries.Where(e => e.IsSelected).ToList();
        if (selected.Count == 0) return "";

        var assetPaths = selected
            .Select(e => AssetDatabase.GetAssetPath(e.Texture))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        return FindCommonFolder(assetPaths);
    }

    private static string FindCommonFolder(List<string> paths)
    {
        if (paths.Count == 0) return "Assets";
        var segments = paths.Select(p => p.Split('/')).ToArray();
        int minLen = segments.Min(s => s.Length);
        int common = 0;
        while (common < minLen && segments.All(s => s[common] == segments[0][common])) common++;
        var result = string.Join("/", segments[0].Take(common));
        if (System.IO.Path.HasExtension(result))
            result = System.IO.Path.GetDirectoryName(result)?.Replace('\\', '/') ?? "Assets";
        return string.IsNullOrEmpty(result) ? "Assets" : result;
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
    private static readonly Comparison<TextureEntry> SHARED_SORT = (a, b) => a.SharedContextCount.CompareTo(b.SharedContextCount);
    private static readonly Comparison<TextureEntry> FOLDER_SORT = (a, b) => string.Compare(a.FolderPath, b.FolderPath, StringComparison.Ordinal);
}
