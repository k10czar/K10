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
    private Dictionary<string, SpriteAtlas> _atlasLookupCache;
    private Comparison<TextureEntry> _lastComparison;
    private readonly List<(Comparison<TextureEntry> base_, bool descending)> _sortStack = new List<(Comparison<TextureEntry>, bool)>();
    private static readonly System.Reflection.MethodInfo _getPreviewTextures =
        typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

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
            long atlasSize = 0;
            if (_getPreviewTextures?.Invoke(null, new object[] { atlas }) is Texture2D[] pages)
                foreach (var page in pages)
                    if (page != null) atlasSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(page);
            _atlasMemories[atlas.name] = atlasSize;
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
                var (inAtlas, atlasObj) = GetAtlasInfo(img.sprite, lookup);
                entry = new TextureEntry(tex, inAtlas, atlasObj);
                texDict[tex] = entry;
            }
            entry.AddUser(img);
        }
        foreach (var raw in ctx.Target.GetComponentsInChildren<RawImage>(_includeDisabled))
        {
            if (raw == null || raw.texture == null) continue;
            if (!texDict.TryGetValue(raw.texture, out var entry))
            {
                entry = new TextureEntry(raw.texture, false, null);
                texDict[raw.texture] = entry;
            }
            entry.AddUser(raw);
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

    private static Dictionary<string, SpriteAtlas> BuildAtlasLookup()
    {
        var lookup = new Dictionary<string, SpriteAtlas>(StringComparer.OrdinalIgnoreCase);
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
                        lookup[AssetDatabase.GUIDToAssetPath(sg)] = atlas;
                }
                else lookup[path] = atlas;
            }
        }
        return lookup;
    }

    private static (bool inAtlas, SpriteAtlas atlasObj) GetAtlasInfo(Sprite sprite, Dictionary<string, SpriteAtlas> atlasLookup)
    {
        var path = AssetDatabase.GetAssetPath(sprite);
        if (!string.IsNullOrEmpty(path) && atlasLookup.TryGetValue(path, out var atlas))
            return (true, atlas);
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

        var seenTextures = new HashSet<Texture>();
        var countedAtlases = new HashSet<string>();
        long globalMem = 0;
        foreach (var ctx in _contexts)
            foreach (var e in ctx.Entries)
                if (seenTextures.Add(e.Texture))
                {
                    if (e.InAtlas)
                    { if (!string.IsNullOrEmpty(e.AtlasName) && countedAtlases.Add(e.AtlasName) && _atlasMemories.TryGetValue(e.AtlasName, out var am)) globalMem += am; }
                    else globalMem += e.MemorySize;
                }
        var globalLabel = _contexts.Count > 0 ? $"Targets ({_targets.Count})  |  {seenTextures.Count} unique tex  |  {FormatMemory(globalMem)}" : $"Targets ({_targets.Count})";

        EditorGUILayout.BeginHorizontal();
        _showTargets = EditorGUILayout.BeginFoldoutHeaderGroup( _showTargets, globalLabel );
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
        var atlasNameW = GUILayout.Width(150);
        var enabledW = GUILayout.Width(75);
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
        if (GUILayout.Button("Atlas", atlasNameW)) ToggleSortAll(ATLAS_NAME_SORT);
        if (GUILayout.Button("Enabled", enabledW)) ToggleSortAll(ENABLED_SORT);
        if (GUILayout.Button("Uses", usageW)) ToggleSortAll(USAGE_SORT);
        var showShared = _contexts.Count > 1;
        if (showShared && GUILayout.Button("Shared", sharedW)) ToggleSortAll(SHARED_SORT);
        if (GUILayout.Button("Folder")) ToggleSortAll(FOLDER_SORT);
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        int removeIndex = -1;
        for (int ci = 0; ci < _contexts.Count; ci++)
        {
            var ctx = _contexts[ci];
            if (DrawContextHeader(ctx)) { removeIndex = ci; break; }
            if (!ctx.Foldout) continue;
            DrawSelectionHeader(ctx);
            for (int i = 0; i < ctx.Entries.Count; i++)
            {
                var (clicked, shift) = ctx.Entries[i].Draw(i, numW, texW, dimW, memW, potW, mipW, atlasNameW, enabledW, usageW, sharedW, folderW, showShared);
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

    private void DrawSelectionHeader(ContextData ctx)
    {
        var selCount = ctx.SelectedCount;
        if (selCount == 0) return;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("✕", GUILayout.Width(18), GUILayout.Height(16)))
            foreach (var e in ctx.Entries) e.IsSelected = false;
        var selectedSize = ctx.Entries.Where(e => e.IsSelected).Sum(e => e.MemorySize);
        GUILayout.Label($"{selCount} selected\n{FormatMemory(selectedSize)}", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Add to Atlas ▾", GUILayout.ExpandWidth(false)))
        {
            var capturedCtx = ctx;
            var menu = new GenericMenu();
            foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                var atlasAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlasAsset == null) continue;
                menu.AddItem(new GUIContent(atlasAsset.name), false, () =>
                {
                    var toAdd = capturedCtx.Entries
                        .Where(e => e.IsSelected)
                        .Select(e => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(e.Texture)))
                        .Where(o => o != null)
                        .ToArray();
                    if (toAdd.Length == 0) return;
                    foreach (var e in capturedCtx.Entries.Where(e => e.IsSelected && e.AtlasObject != null && e.AtlasObject != atlasAsset))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(e.Texture));
                        if (obj != null) e.AtlasObject.Remove(new[] { obj });
                        EditorUtility.SetDirty(e.AtlasObject);
                    }
                    atlasAsset.Add(toAdd);
                    EditorUtility.SetDirty(atlasAsset);
                    AssetDatabase.SaveAssets();
                    foreach (var e in capturedCtx.Entries.Where(e => e.IsSelected))
                    {
                        e.InAtlas = true;
                        e.AtlasObject = atlasAsset;
                    }
                });
            }
            if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("No SpriteAtlas found"));
            menu.ShowAsContext();
        }

        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label("Create Atlas:", MiniLabelMiddle, GUILayout.ExpandWidth(false));
        var savedFolder = _createAtlasFolder.Get;
        var isCustom = !string.IsNullOrEmpty(savedFolder);
        var effectiveFolder = isCustom ? savedFolder : FindCommonFolder(ctx);
        var displayFolder = effectiveFolder.Length > 90 ? "…" + effectiveFolder[^87..] : effectiveFolder;
        if (GUILayout.Button($"@ {displayFolder} ▾", GUILayout.ExpandWidth(false)))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Common Parent Folder"), !isCustom, () => _createAtlasFolder.Set = string.Empty);
            menu.AddItem(new GUIContent("Custom Folder…"), isCustom, () =>
            {
                var folder = EditorUtility.OpenFolderPanel("Select Atlas Save Folder", isCustom ? savedFolder : "Assets", "");
                if (string.IsNullOrEmpty(folder)) return;
                if (folder.StartsWith(Application.dataPath))
                    folder = "Assets" + folder[Application.dataPath.Length..];
                _createAtlasFolder.Set = folder;
            });
            menu.ShowAsContext();
        }
        GuiColorManager.New(new Color(.55f, 1f, 0.55f));
        if (GUILayout.Button("🌟V2", GUILayout.ExpandWidth(false))) CreateAtlasV2(ctx, effectiveFolder);
        GuiColorManager.New(new Color(1f, .8f, 0.55f));
        if (GUILayout.Button("V1", GUILayout.ExpandWidth(false))) CreateAtlas(ctx, effectiveFolder);
        GuiColorManager.Revert(2);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
            Selection.objects = ctx.Entries.Where(e => e.IsSelected).Select(e => e.Texture).ToArray<UnityEngine.Object>();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();
    }

    private static readonly Color _ctxHeaderBg = new Color(0.2f, 0.3f, 0.5f, 0.4f);
    private static readonly EditorPersistentString _createAtlasFolder = EditorPersistentString.At( "UiTexturesReporter/CreateAtlasFolder" );
    private static GUIStyle _miniLabelMiddle;
    private static GUIStyle MiniLabelMiddle => _miniLabelMiddle ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };

    private bool DrawContextHeader(ContextData ctx)
    {
        var rect = EditorGUILayout.BeginHorizontal();
        if (Event.current.type == EventType.Repaint) EditorGUI.DrawRect(rect, _ctxHeaderBg);
        var selCount = ctx.SelectedCount;
        var selLabel = selCount > 0 ? $"  [{selCount} selected]" : "";
        var prevFoldout = ctx.Foldout;
        ctx.Foldout = EditorGUILayout.Foldout(ctx.Foldout, $"{ctx.Entries.Count} tex  |  {FormatMemory(ComputeTotalMemory(ctx.Entries))}  |  {ctx.Target.NameOrNull()}{selLabel}");
        if (ctx.Foldout != prevFoldout && Event.current.shift)
            foreach (var c in _contexts) c.Foldout = ctx.Foldout;
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
        public bool InAtlas;
        public SpriteAtlas AtlasObject;
        public string AtlasName => AtlasObject != null ? AtlasObject.name : string.Empty;
        public readonly long MemorySize;
        public readonly bool IsPowerOfTwo;
        public readonly bool HasMipMap;
        public int SharedContextCount = 1;
        public bool IsSelected;
        public int EnabledCount;
        public readonly string FolderPath;
        private readonly List<MonoBehaviour> _users = new List<MonoBehaviour>();

        public int UsageCount => _users.Count;

        public TextureEntry(Texture texture, bool inAtlas, SpriteAtlas atlasObj)
        {
            EnabledCount = 0;
            Texture = texture;
            InAtlas = inAtlas;
            AtlasObject = atlasObj;
            MemorySize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            IsPowerOfTwo = IsPot(texture.width) && IsPot(texture.height);
            HasMipMap = texture.mipmapCount > 1;
            var assetPath = AssetDatabase.GetAssetPath(texture);
            FolderPath = string.IsNullOrEmpty(assetPath) ? string.Empty : System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? string.Empty;
        }

        private static bool IsPot(int v) => v > 0 && (v & (v - 1)) == 0;

        public void AddUser(MonoBehaviour mb)
        {
            _users.Add(mb);
            if( mb.isActiveAndEnabled ) EnabledCount++;
        }

        private static readonly Color _rowEven = new Color(0f, 0f, 0f, 0f);
        private static readonly Color _rowOdd = new Color(0f, 0f, 0f, 0.08f);
        private static readonly Color _rowSelected = new Color(0.2f, 0.5f, 0.9f, 0.25f);
        private static readonly Color _rowNoneEnabled = new Color(0f, 0f, 0f, 0.25f);
        private static readonly Color _rowSomeEnabled = new Color(0.8f, 0.5f, 0, 0.05f);
        private static readonly Color _rowAllEnabled = new Color(0.1f, 0.8f, 0, 0.05f);

        public (bool clicked, bool shift) Draw(int index, GUILayoutOption numW, GUILayoutOption texW, GUILayoutOption dimW, GUILayoutOption memW, GUILayoutOption potW, GUILayoutOption mipW, GUILayoutOption atlasNameW, GUILayoutOption enabledW, GUILayoutOption usageW, GUILayoutOption sharedW, GUILayoutOption folderW, bool showShared)
        {
            bool clicked = false;
            bool shift = false;

            var enabledCount = EnabledCount;
            var total = UsageCount;

            var allEnabled = enabledCount == total;
            var noneEnabled = enabledCount == 0;
            
            var rect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, index % 2 == 0 ? _rowEven : _rowOdd);
                if (IsSelected) EditorGUI.DrawRect(rect, _rowSelected);
                else if( noneEnabled ) EditorGUI.DrawRect(rect, _rowNoneEnabled);
                else if( !allEnabled ) EditorGUI.DrawRect(rect, _rowSomeEnabled);
                else EditorGUI.DrawRect(rect, _rowAllEnabled);
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
            GuiColorManager.Revert(2);
            if( AtlasObject != null )
                EditorGUILayout.ObjectField(AtlasObject, typeof(SpriteAtlas), false, atlasNameW);
            else if (GUILayout.Button("+ Atlas ▾", atlasNameW))
            {
                var capturedTex = Texture;
                var menu = new GenericMenu();
                foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
                {
                    var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                    var atlasAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                    if (atlasAsset == null) continue;
                    var capturedEntry = this;
                    menu.AddItem(new GUIContent(atlasAsset.name), false, () =>
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(capturedTex));
                        if (obj == null) return;
                        atlasAsset.Add(new[] { obj });
                        EditorUtility.SetDirty(atlasAsset);
                        AssetDatabase.SaveAssets();
                        capturedEntry.InAtlas = true;
                        capturedEntry.AtlasObject = atlasAsset;
                    });
                }
                if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("No SpriteAtlas found"));
                menu.ShowAsContext();
            }
            
            if (allEnabled) GuiColorManager.New(Color.green);
            else if (!noneEnabled) GuiColorManager.New(Color.yellow);
            else GuiColorManager.New(Color.gray);

            if( noneEnabled ) EditorGUI.BeginDisabledGroup(noneEnabled);
            
            var enabledLabel = allEnabled ? "All" : noneEnabled ? "None"
                : $"{enabledCount} ({Mathf.RoundToInt(100f * enabledCount / total)}%)";
            if (GUILayout.Button(enabledLabel, enabledW))
                Selection.objects = _users.Where(mb => mb != null && mb.isActiveAndEnabled).Select(mb => mb.gameObject).ToArray<UnityEngine.Object>();
            
            if( noneEnabled ) EditorGUI.EndDisabledGroup();

            GuiColorManager.Revert();
            if (GUILayout.Button(total.ToString(), usageW))
                Selection.objects = _users.Select(mb => mb.gameObject).ToArray<UnityEngine.Object>();
            if (showShared)
            {
                var isExclusive = SharedContextCount <= 1;
                GuiColorManager.New(isExclusive ? Color.green : Color.yellow);
                EditorGUILayout.LabelField(isExclusive ? "Exclusive" : $"{SharedContextCount} ctx", sharedW);
                GuiColorManager.Revert();
            }
            EditorGUILayout.LabelField(FolderPath);
            EditorGUILayout.EndHorizontal();
            return (clicked, shift);
        }
    }

    private static void CreateAtlas(ContextData ctx, string folder = null)
    {
        var selected = ctx.Entries.Where(e => e.IsSelected).ToList();
        if (selected.Count == 0) return;

        var assetPaths = selected
            .Select(e => AssetDatabase.GetAssetPath(e.Texture))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        folder ??= FindCommonFolder(assetPaths);
        var atlasName = ctx.Target != null ? ctx.Target.name : "NewAtlas";
        var atlasPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{atlasName}.spriteatlas");

        var atlas = new SpriteAtlas();
        var packables = assetPaths
            .Select(p => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p))
            .Where(o => o != null)
            .ToArray();
        atlas.Add(packables);

        var ps = atlas.GetPackingSettings();
        ps.enableRotation = false;
        ps.enableTightPacking = false;
        atlas.SetPackingSettings(ps);

        AssetDatabase.CreateAsset(atlas, atlasPath);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(atlas);
    }

    private static void CreateAtlasV2(ContextData ctx, string folder = null)
    {
        var selected = ctx.Entries.Where(e => e.IsSelected).ToList();
        if (selected.Count == 0) return;

        var assetPaths = selected
            .Select(e => AssetDatabase.GetAssetPath(e.Texture))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        folder ??= FindCommonFolder(assetPaths);
        var atlasName = ctx.Target != null ? ctx.Target.name : "NewAtlas";
        var atlasPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{atlasName}.spriteatlasv2");

        var atlas = new SpriteAtlasAsset();
        var packables = assetPaths
            .Select(p => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p))
            .Where(o => o != null)
            .ToArray();
        if (packables.Length > 0) atlas.Add(packables);

        AssetDatabase.CreateAsset(atlas, atlasPath);
        AssetDatabase.SaveAssets();

        var importer = AssetImporter.GetAtPath(atlasPath) as SpriteAtlasImporter;
        if (importer != null)
        {
            var ps = importer.packingSettings;
            ps.enableRotation = false;
            ps.enableTightPacking = false;
            importer.packingSettings = ps;
            importer.SaveAndReimport();
        }

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(atlasPath));
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
    private static readonly Comparison<TextureEntry> ENABLED_SORT = (a, b) => {
        float ra = a.UsageCount > 0 ? (float)a.EnabledCount / a.UsageCount : 0f;
        float rb = b.UsageCount > 0 ? (float)b.EnabledCount / b.UsageCount : 0f;
        return ra.CompareTo(rb);
    };
    private static readonly Comparison<TextureEntry> USAGE_SORT = (a, b) => a.UsageCount.CompareTo(b.UsageCount);
    private static readonly Comparison<TextureEntry> SHARED_SORT = (a, b) => a.SharedContextCount.CompareTo(b.SharedContextCount);
    private static readonly Comparison<TextureEntry> FOLDER_SORT = (a, b) => string.Compare(a.FolderPath, b.FolderPath, StringComparison.Ordinal);
}
