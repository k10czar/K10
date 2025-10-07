using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using K10.EditorGUIExtention;
using System.Linq;

public class ImageDensityReporter : EditorWindow
{
    const string FILE_EXTENSION = "kirl";
    const string REPORT_NAME = "Report";

    InfoNode _root = new InfoNode( REPORT_NAME );
    private bool _showHierarchy = false;
    private bool _showReportCount = false;
    private bool _tickOnHierarchyChange = true;
    private bool _tickOnUpdate = false;
    private Vector2 _scroll;

    private System.Diagnostics.Stopwatch _lastReportMetrics;

    private readonly SortingField<InfoNode> _sort = new SortingField<InfoNode>( new (Comparison<InfoNode>,string)[]{ ( InfoNode.NAME_SORT, "Name" ), ( InfoNode.MINDENSITY_SORT, "MinDensity"), ( InfoNode.MAXDENSITY_SORT, "MaxDensity"), ( InfoNode.REPORTS_SORT, "Reports"), ( InfoNode.NODES_COUNT_SORT, "Childs") } );

    [MenuItem("K10/Reports/ImageDensity")] private static void Init() { GetWindow<ImageDensityReporter>( "Image Density Reporter" ); }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHirerchyChanges;
        OnHirerchyChanges();
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHirerchyChanges;
    }

    static readonly float[] DENSITY_RANGES = new float[]{ .75f, 1, 2, 5 };
    static readonly Color[] DENSITY_COLORS = new Color[]{ Colors.LightSalmon, Colors.LightSkyBlue, Colors.KeyLime, Colors.LightCoral };

    public static Color GetDensityColor( float density ) => ColorUtils.Lerp( DENSITY_COLORS, density, DENSITY_RANGES );

    private void OnHirerchyChanges()
    {
        if( !_tickOnHierarchyChange ) return;
        Report();
    }

    private void Report()
    {
        _lastReportMetrics = new System.Diagnostics.Stopwatch();
        _lastReportMetrics.Start();

        var objs = FindObjectsOfType<Image>();

        foreach (var uiImg in objs)
        {
            if( uiImg == null ) continue;
            if( uiImg.type != Image.Type.Simple ) continue;
            var sprite = uiImg.sprite;
            if( sprite == null ) continue;
            // var texture = sprite.texture;
            // if( texture == null ) continue;
            var path = AssetDatabase.GetAssetPath( sprite );
            if( string.IsNullOrEmpty( path ) ) path = $"Dynamic/{sprite.NameOrNull()}.sprite";
            var pathSplitted = path.Split( "/" );

            var sRect = sprite.rect;
            var TextureSize = sprite != null ? new Vector2( sRect.width, sRect.height ) : Vector2.zero;

            var rt = uiImg.rectTransform;
            var rect = rt.rect;
            var scl = rt.lossyScale;
            var eSize = new Vector2( rect.width * scl.x, rect.height * scl.y );
            var ExhibitionDensity = new Vector2( TextureSize.x / eSize.x, TextureSize.y / eSize.y );
            var maxDensity = Mathf.Max( ExhibitionDensity.x, ExhibitionDensity.y );
            var minDensity = uiImg.preserveAspect ? maxDensity : Mathf.Min( ExhibitionDensity.x, ExhibitionDensity.y );
            // var pixels = TextureSize.x * TextureSize.y;
            _root.Report( pathSplitted, 0, minDensity, maxDensity, sprite.texture, uiImg );
        }
        _lastReportMetrics.Stop();
    }

    void Update()
    {
        if( _tickOnUpdate ) Report();
        Repaint();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        if( GUILayout.Button( "Report", GUILayout.Width( 80 ) ) ) Report();
        if( GUILayout.Button( "Clear", GUILayout.Width( 80 ) ) ) _root.Clear( REPORT_NAME );
        EditorGUILayout.EndHorizontal();
        var height = position.height - ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing );
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        GuiUtils.Label.ExactSizeLayout( "File:" );
        if( GUILayout.Button( "Save", GUILayout.Width( 80 ) ) ) Save();
        if( GUILayout.Button( "Load", GUILayout.Width( 80 ) ) ) Load();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        GuiUtils.Label.ExactSizeLayout( "Debug:" );
        GuiUtils.Toggle.ExactSizeLayout( ref _showHierarchy, "Hierarchy" );
        GuiUtils.Toggle.ExactSizeLayout( ref _showReportCount, "Reports" );
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        GuiUtils.Label.ExactSizeLayout( "Auto Report:" );
        GuiUtils.Toggle.ExactSizeLayout( ref _tickOnHierarchyChange, "On Hierarchy Change" );
        GuiUtils.Toggle.ExactSizeLayout( ref _tickOnUpdate, "On Update" );
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal( GUI.skin.box );
        if( _lastReportMetrics != null ) GuiUtils.Label.ExactSizeLayout( $"{_lastReportMetrics.Elapsed.TotalMilliseconds:N2}ms", Colors.GreenEarth );
        EditorGUILayout.EndHorizontal();
        _sort.LayoutAsDropdown( GUILayout.Width( 170 ) );
        // _sort.LayoutAsButtonList();
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView( _scroll );
        GuiUtils.Scroll.CalculateLinesToDraw( _scroll, height, out var startId, out var lines );
        GuiUtils.Scroll.DrawFakeLines( startId );
        int id = 0;
        _root.Draw( ref id, startId, lines, _showHierarchy, _showReportCount, _sort.ComparisonOrder );
        var linesCount = _root.CountLines();
        GuiUtils.Scroll.DrawFakeLines( linesCount - ( startId + lines ) );
        EditorGUILayout.EndScrollView();
    }

    void Load()
    {
        var initialPath = $"{Application.dataPath.Replace("/Assets","")}/Logs";
        // Debug.Log( $"Load( {initialPath} )" );
        var path = EditorUtility.OpenFilePanel( "Load Image Report Log", initialPath, FILE_EXTENSION );
        if( path.Length == 0 ) return;
        var bytes = FileAdapter.ReadAllBytes( path );
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        _root = JsonUtility.FromJson<InfoNode>( json );
        var fileName = path.Split( "/" ).Last();
        _root.ChageName( fileName );
    }

    void Save()
    {
        var currentDateTime = DateTime.Now;
        var dateStr = currentDateTime.ToString("yyyyMMdd_HHmmss");
        var fileName = $"ImageReportLog_{dateStr}";
        var initialPath = $"{Application.dataPath.Replace("/Assets","")}/Logs";
        // Debug.Log( $"Save( {initialPath} )" );
        var path = EditorUtility.SaveFilePanel( "Save Image Report Log", initialPath, $"{fileName}.{FILE_EXTENSION}", FILE_EXTENSION );
        if( path.Length == 0 ) return;
        var serializedData = JsonUtility.ToJson( _root );
        var bytes = System.Text.Encoding.UTF8.GetBytes(serializedData);
        FileAdapter.WriteAllBytes( path, bytes );
    }

    [System.Serializable]
    public class InfoNode : ISerializationCallbackReceiver
    {
        [SerializeField] string _name;
        [SerializeField] float _minDensity;
        [SerializeField] float _maxDensity;
        [SerializeField] bool _expanded = true;
        [SerializeField] ulong _reports;
        bool _readed = false;
        Texture2D _texture;
        [SerializeField] string _guid;
        [SerializeField] string _hierarchy;
        [SerializeField] string[] _serializedKeys;
        [SerializeField] InfoNode[] _serializedValues;
        Dictionary<string,InfoNode> _nodes = new Dictionary<string,InfoNode>();

        public static readonly Comparison<InfoNode> NAME_SORT = ( InfoNode a, InfoNode b ) => a._name?.CompareTo( b._name ) ?? 1;
        public static readonly Comparison<InfoNode> MINDENSITY_SORT = ( InfoNode a, InfoNode b ) => a._minDensity.CompareTo( b._minDensity );
        public static readonly Comparison<InfoNode> MAXDENSITY_SORT = ( InfoNode a, InfoNode b ) => a._maxDensity.CompareTo( b._maxDensity );
        public static readonly Comparison<InfoNode> REPORTS_SORT = ( InfoNode a, InfoNode b ) => a._reports.CompareTo( b._reports );
        public static readonly Comparison<InfoNode> NODES_COUNT_SORT = ( InfoNode a, InfoNode b ) => ( a._nodes?.Count ?? 0).CompareTo( b._nodes?.Count ?? 0 );

        private static readonly GUILayoutOption _ScaleWidth = GUILayout.Width( 100 );

        public void OnAfterDeserialize()
        {
            if( _serializedKeys != null && _serializedValues != null && _serializedKeys.Length == _serializedValues.Length )
            {
                if( _nodes == null ) _nodes = new Dictionary<string, InfoNode>();
                else _nodes.Clear();
                int n = _serializedKeys.Length;
                for(int i = 0; i < n; ++i)
                {
                    _nodes[_serializedKeys[i]] = _serializedValues[i];
                }

                _serializedKeys = null;
                _serializedValues = null;
            }

            _readed = false;
        }

        void TryReadTexture()
        {
            if( _readed ) return;
            if( string.IsNullOrEmpty( _guid ) ) return;
            var path = AssetDatabase.GUIDToAssetPath( _guid );
            _texture = AssetDatabase.LoadAssetAtPath<Texture2D>( path );
        }

        public void OnBeforeSerialize()
        {
            int n = _nodes.Count;
            _serializedKeys = new string[n];
            _serializedValues = new InfoNode[n];

            int i = 0;
            foreach(var kvp in _nodes)
            {
                _serializedKeys[i] = kvp.Key;
                _serializedValues[i] = kvp.Value;
                ++i;
            }

            _guid = null;
            if( _texture != null )
            {
                _guid = AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( _texture ) );
            }
        }

        public  void ChageName( string newName )
        {
            _name = newName;
        }

        public InfoNode( string name )
        {
            Clear( name );
        }

        public void Draw( ref int id, int startId, int lines, bool showHierarchy, bool showReportsCounter, Comparison<InfoNode> order )
        {
            if( id > startId + lines ) return;
            if( id >= startId )
            {
                EditorGUILayout.BeginHorizontal();
                var hasChilds = _nodes != null && _nodes.Count > 0;
                // var label = ( !_expanded || !hasChilds ) ? $"{_name} {_scale:N2}" : _name;
                var label = _name;
                if( hasChilds )
                {
                    var newFold = EditorGUILayout.Foldout( _expanded, label );
                    if( _expanded != newFold )
                    {
                        var recursive = EditorGUI.actionKey;
                        ToggleExpand( recursive );
                    }
                }
                else
                {
                    TryReadTexture();
                    if( _texture != null )
                    {
                        // EditorGuiIndentManager.New( 0 );
                        EditorGUILayout.ObjectField( _texture, typeof(Texture2D), true );
                        // EditorGuiIndentManager.Revert();
                    }
                    else
                    {
                        EditorGUILayout.LabelField( label );
                    }
                }
                if( showHierarchy && !string.IsNullOrEmpty( _hierarchy ) ) GuiUtils.Label.ExactSizeLayout( _hierarchy, Colors.Silver );
                if( showReportsCounter ) GuiUtils.Label.ExactSizeLayout( _reports.ToString(), Colors.Thistle );
                var drawScale = ( !_expanded || !hasChilds );
                if( drawScale )
                {
                    var differentDensities = !Mathf.Approximately( _minDensity, _maxDensity );
                    if( differentDensities )
                    {
                        GuiUtils.Label.ExactSizeLayout( _minDensity.ToString("N2"), GetDensityColor( _minDensity ) );
                        GuiUtils.Label.ExactSizeLayout( "..." );
                        GuiUtils.Label.ExactSizeLayout( _maxDensity.ToString("N2"), GetDensityColor( _maxDensity ) );
                    }
                    else
                    {
                        GuiUtils.Label.ExactSizeLayout( _minDensity.ToString("N2"), GetDensityColor( _minDensity ) );
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            id++;
            if( !_expanded ) return;
            if( _nodes != null )
            {
                EditorGuiIndentManager.New( EditorGUI.indentLevel + 1 );
                var orderedNodes = _nodes.Values.ToList();
                orderedNodes.Sort( order );
                foreach( var node in orderedNodes ) node.Draw( ref id, startId, lines, showHierarchy, showReportsCounter, order );
                EditorGuiIndentManager.Revert();
            }
        }

        public void ToggleExpand( bool recursive )
        {
            Expand( !_expanded, recursive );
        }

        public void Expand( bool newExpandValue, bool recursive )
        {
            _expanded = newExpandValue;
            if( recursive )
            {
                foreach( var node in _nodes )
                    node.Value.Expand( newExpandValue, true );
            }
        }

        public void Report( string[] path, int pathID, float minDensity, float maxDensity, Texture2D texture, Image image )
        {
            _reports++;
            var lastNode = pathID >= path.Length;
            if( Mathf.Abs( minDensity ) < Mathf.Abs( _minDensity ) )
            {
                _minDensity = minDensity;
                if( lastNode ) _hierarchy = image.transform.HierarchyNameOrNull();
            }
            if( Mathf.Abs( maxDensity ) > Mathf.Abs( _maxDensity ) )
            {
                _maxDensity = maxDensity;
            }
            if( lastNode )
            {
                _texture = texture;
                // if( texture != null ) _guid = AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( _texture ) );
                // else _guid = null;
                return;
            }
            var currentPathNode = path[ pathID ];
            if( !_nodes.TryGetValue( currentPathNode, out var node ) )
            {
                node = new InfoNode( currentPathNode );
                _nodes.Add( currentPathNode, node );
            }
            node.Report( path, pathID + 1, minDensity, maxDensity, texture, image );
        }

        public void Clear( string name )
        {
            _name = name;
            _nodes.Clear();
            _minDensity = float.PositiveInfinity;
            _maxDensity = 0;
            _reports = 0;
            _hierarchy = null;
        }

        public int CountLines()
        {
            int count = 1;
            if( _expanded && _nodes != null ) foreach( var node in _nodes ) count += node.Value.CountLines();
            return count;
        }
    }
}