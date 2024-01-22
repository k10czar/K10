using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ImagesMonitor : EditorWindow
{
    List<ImageInfo> observeds = new List<ImageInfo>();
    private bool _includeInactive = false;
    private Vector2 _scroll;
    private Comparison<ImageInfo> _lastComparison;

    [MenuItem("K10/Monitor/Images")] private static void Init() { GetWindow<ImagesMonitor>( "Images Monitor" ); }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += UpdateSprites;
        UpdateSprites();
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= UpdateSprites;
    }

    private void UpdateSprites()
    {
        var objs = FindObjectsOfType<Image>( _includeInactive );

        observeds.Clear();
        foreach (var sr in objs)
        {
            if( sr == null ) continue;
            if( sr.type != Image.Type.Simple ) continue;
            if( !_includeInactive && !sr.enabled ) continue;
            observeds.Add( new ImageInfo( sr ) );
        }

        Sort( _lastComparison );
    }

    void Update()
    {
        Repaint();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if( GUILayout.Button( "Update", GUILayout.Width( 80 ) ) ) UpdateSprites();
        var includeInactive = GUILayout.Toggle( _includeInactive, "Show Inactives", GUILayout.Width( 120 ) );
        if( _includeInactive != includeInactive )
        {
            _includeInactive = includeInactive;
            UpdateSprites();
        }
        // GUILayout.Label( position.ToString() );
        var height = position.height - ( EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ) * 2;
        GuiUtils.Scroll.CalculateLinesToDraw( _scroll, height, out var startId, out var lines );
        // GUILayout.Label( $"_scroll:{_scroll} height:{height} startId:{startId} lines:{lines}" );
        EditorGUILayout.LabelField( $"{observeds.Count}" );
        EditorGUILayout.EndHorizontal();

        var refW = GUILayout.Width( 240 );
        var dimensionW = GUILayout.Width( 72 );
        var densityW = GUILayout.Width( 92 );
        var textureW = GUILayout.Width( 240 );
        var sizeW = GUILayout.Width( 120 );
        
        EditorGUILayout.BeginHorizontal();
        if( GUILayout.Button( "Image", refW ) ) SortByImage();
        if( GUILayout.Button( "Width", dimensionW ) ) SortByScreenWidth();
        if( GUILayout.Button( "Height", dimensionW ) ) SortByScreenHeight();
        if( GUILayout.Button( "Texture", textureW ) ) SortByTexture();
        if( GUILayout.Button( "Width", dimensionW ) ) SortByTextureWidth();
        if( GUILayout.Button( "Height", dimensionW ) ) SortByTextureHeight();
        if( GUILayout.Button( "Density X", dimensionW ) ) SortByDensityX();
        if( GUILayout.Button( "Density Y", dimensionW ) ) SortByDensityY();
        if( GUILayout.Button( "Min Density", densityW ) ) SortByMinDensity();
        if( GUILayout.Button( "Max Density", densityW ) ) SortByMaxDensity();
        if( GUILayout.Button( "File Size", sizeW ) ) SortBySize();
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView( _scroll );
        GuiUtils.Scroll.DrawFakeLines( startId );
        for( int i = startId; i < observeds.Count && i < startId + lines; i++ )
        {
            observeds[i].Draw( refW, dimensionW, textureW, sizeW, densityW );
        }
        GuiUtils.Scroll.DrawFakeLines( observeds.Count - ( startId + lines ) );
        EditorGUILayout.EndScrollView();
    }

    private void Sort( Comparison<ImageInfo> comparison )
    {
        if( comparison == null ) return;
        _lastComparison = comparison;
        observeds.Sort( _lastComparison );
    }

    private void ToggleSort( Comparison<ImageInfo> comparison )
    {
        if( comparison == null ) return;
        comparison.SetOrRevertOn( ref _lastComparison );
        observeds.Sort( _lastComparison );
    }

    private void SortByImage() { ToggleSort( IMAGE_NAME_SORT ); }
    private void SortByScreenWidth() { ToggleSort( SCREEN_WIDTH_SORT ); }
    private void SortByScreenHeight() { ToggleSort( SCREEN_HEIGHT_SORT ); }
    private void SortByTexture() { ToggleSort( TEXTURE_SORT_SORT ); }
    private void SortByTextureWidth() { ToggleSort( TEXTURE_WIDTH_SORT ); }
    private void SortByTextureHeight() { ToggleSort( TEXTURE_HEIGHT_SORT ); }
    private void SortByDensityX() { ToggleSort( DENSITY_X_SORT ); }
    private void SortByDensityY() { ToggleSort( DENSITY_Y_SORT ); }
    private void SortByMinDensity() { ToggleSort( DENSITY_MIN_SORT ); }
    private void SortByMaxDensity() { ToggleSort( DENSITY_MAX_SORT ); }
    private void SortBySize() { ToggleSort( FILE_SIZE_SORT ); }

    public class ImageInfo
    {
        public readonly Image image;

        private AnalizedData cachedData;

        public struct AnalizedData
        {
            public readonly Sprite Sprite;
            public readonly Texture2D Texture;
            public readonly Vector2 ExhibitionSize;
            public readonly Vector2 TextureSize;
            public readonly Vector2 ExhibitionDensity;
            public readonly float MinDensity;
            public readonly float MaxDensity;
            public readonly long FileSize;

            public AnalizedData( Sprite sprite, Vector2 exhibitionSize, bool preserveAspect )
            {
                Sprite = sprite;
                Texture = sprite != null ? sprite.texture : null;
                TextureSize = sprite != null ? new Vector2( sprite.rect.width, sprite.rect.height ) : Vector2.zero;
                ExhibitionSize = exhibitionSize;
                ExhibitionDensity = new Vector2( TextureSize.x / ExhibitionSize.x, TextureSize.y / ExhibitionSize.y );
                MaxDensity = Mathf.Max( ExhibitionDensity.x, ExhibitionDensity.y );
                if( preserveAspect )
                {
                    ExhibitionDensity.x = MaxDensity;
                    ExhibitionDensity.y = MaxDensity;
                    MinDensity = MaxDensity;
                }
                else
                {
                    MinDensity = Mathf.Min( ExhibitionDensity.x, ExhibitionDensity.y );
                }
                FileSize = Texture != null ? UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong( Texture ) : 0;
            }
        }

        public AnalizedData Data
        {
            get
            {
                var sprite = image.sprite;
                var rt = image.rectTransform;
                var rect = rt.rect;
                var scl = rt.lossyScale;
                var eSize = new Vector2( rect.width * scl.x, rect.height * scl.y );
                var cachedSize = cachedData.ExhibitionSize;
                if( cachedData.Sprite != sprite || !Mathf.Approximately( eSize.x, cachedSize.x ) || !Mathf.Approximately( eSize.y, cachedSize.y )  ) cachedData = new AnalizedData( sprite, eSize, image.preserveAspect );
                return cachedData;
            }
        }

        public ImageInfo( Image sr )
        {
            this.image = sr;
        }

        public void Draw( GUILayoutOption refOpt, GUILayoutOption dimensionW, GUILayoutOption textureOpt, GUILayoutOption fileSizeOpt, GUILayoutOption densityOpt )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField( image, typeof(SpriteRenderer), true, refOpt );

            var data = Data;
            var size = data.ExhibitionSize;
            EditorGUILayout.LabelField( size.x.ToString( "N0" ), dimensionW );
            EditorGUILayout.LabelField( size.y.ToString( "N0" ), dimensionW );
            EditorGUILayout.ObjectField( data.Texture, typeof(Texture2D), true, textureOpt );
            var tSize = data.TextureSize;
            EditorGUILayout.LabelField( tSize.x.ToString( "N0" ), dimensionW );
            EditorGUILayout.LabelField( tSize.y.ToString( "N0" ), dimensionW );
            var eScale = data.ExhibitionDensity;
            EditorGUILayout.LabelField( eScale.x.ToString( "N2" ), dimensionW );
            EditorGUILayout.LabelField( eScale.y.ToString( "N2" ), dimensionW );
            GuiColorManager.New( ImageDensityReporter.GetDensityColor( data.MinDensity ) );
            EditorGUILayout.LabelField( data.MinDensity.ToString( "N2" ), densityOpt );
            GuiColorManager.Revert();
            GuiColorManager.New( ImageDensityReporter.GetDensityColor( data.MaxDensity ) );
            EditorGUILayout.LabelField( data.MaxDensity.ToString( "N2" ), densityOpt );
            GuiColorManager.Revert();
            EditorGUILayout.LabelField( data.FileSize.ToString( "N0" ), fileSizeOpt );
            EditorGUILayout.EndHorizontal();
        }
    }

    private static readonly Comparison<ImageInfo> IMAGE_NAME_SORT = ( ImageInfo a, ImageInfo b ) => a.image?.name?.CompareTo( b.image.name ) ?? 1;
    private static readonly Comparison<ImageInfo> SCREEN_WIDTH_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionSize.x.CompareTo( b.Data.ExhibitionSize.x );
    private static readonly Comparison<ImageInfo> SCREEN_HEIGHT_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionSize.y.CompareTo( b.Data.ExhibitionSize.y );
    private static readonly Comparison<ImageInfo> TEXTURE_SORT_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.Texture?.name.CompareTo( b.Data.Texture?.name ?? string.Empty ) ?? 1;
    private static readonly Comparison<ImageInfo> TEXTURE_WIDTH_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.TextureSize.x.CompareTo( b.Data.TextureSize.x );
    private static readonly Comparison<ImageInfo> TEXTURE_HEIGHT_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.TextureSize.y.CompareTo( b.Data.TextureSize.y );
    private static readonly Comparison<ImageInfo> DENSITY_X_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionDensity.x.CompareTo( b.Data.ExhibitionDensity.x );
    private static readonly Comparison<ImageInfo> DENSITY_Y_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionDensity.y.CompareTo( b.Data.ExhibitionDensity.y );
    private static readonly Comparison<ImageInfo> DENSITY_MIN_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.MinDensity.CompareTo( b.Data.MinDensity );
    private static readonly Comparison<ImageInfo> DENSITY_MAX_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.MaxDensity.CompareTo( b.Data.MaxDensity );
    private static readonly Comparison<ImageInfo> FILE_SIZE_SORT = ( ImageInfo a, ImageInfo b ) => a.Data.FileSize.CompareTo( b.Data.FileSize );
}