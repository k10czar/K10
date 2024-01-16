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

    [MenuItem("K10/Reports/Images")] private static void Init() { GetWindow<ImagesMonitor>( "Images Monitor" ); }

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
        ScrollUtils.CalculateLinesToDraw( _scroll, height, out var startId, out var lines );
        // GUILayout.Label( $"_scroll:{_scroll} height:{height} startId:{startId} lines:{lines}" );
        EditorGUILayout.EndHorizontal();

        var refW = GUILayout.Width( 240 );
        var dimensionW = GUILayout.Width( 72 );
        var textureW = GUILayout.Width( 240 );
        var sizeW = GUILayout.Width( 120 );
        
        EditorGUILayout.BeginHorizontal();
        if( GUILayout.Button( "Image", refW ) ) SortByImage();
        if( GUILayout.Button( "Width", dimensionW ) ) SortByScreenWidth();
        if( GUILayout.Button( "Height", dimensionW ) ) SortByScreenHeight();
        if( GUILayout.Button( "Texture", textureW ) ) SortByTexture();
        if( GUILayout.Button( "Width", dimensionW ) ) SortByTextureWidth();
        if( GUILayout.Button( "Height", dimensionW ) ) SortByTextureHeight();
        if( GUILayout.Button( "Scale X", dimensionW ) ) SortByScaleX();
        if( GUILayout.Button( "Scale Y", dimensionW ) ) SortByScaleY();
        if( GUILayout.Button( "Min Scale", dimensionW ) ) SortByMinScale();
        if( GUILayout.Button( "Max Scale", dimensionW ) ) SortByMaxScale();
        if( GUILayout.Button( "File Size", sizeW ) ) SortBySize();
        EditorGUILayout.EndHorizontal();

        _scroll = EditorGUILayout.BeginScrollView( _scroll );
        ScrollUtils.DrawFakeLines( startId );
        for( int i = startId; i < observeds.Count && i < startId + lines; i++ )
        {
            observeds[i].Draw( refW, dimensionW, textureW, sizeW );
        }
        ScrollUtils.DrawFakeLines( observeds.Count - ( startId + lines ) );
        EditorGUILayout.EndScrollView();
    }

    private void Sort( Comparison<ImageInfo> comparison )
    {
        if( comparison == null ) return;
        _lastComparison = comparison;
        observeds.Sort( comparison );
    }

    private static readonly Comparison<ImageInfo> IMAGE_SORT_ASC = ( ImageInfo a, ImageInfo b ) => a.image?.name.CompareTo( b.image.name ) ?? 1;
    private static readonly Comparison<ImageInfo> IMAGE_SORT_DESC = ( ImageInfo a, ImageInfo b ) => b.image?.name.CompareTo( a.image.name ) ?? 1;
    private static readonly Comparison<ImageInfo> SCREEN_WIDTH_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionSize.x.CompareTo( b.Data.ExhibitionSize.x );
    private static readonly Comparison<ImageInfo> SCREEN_WIDTH_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.ExhibitionSize.x.CompareTo( a.Data.ExhibitionSize.x );
    private static readonly Comparison<ImageInfo> SCREEN_HEIGHT_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionSize.y.CompareTo( b.Data.ExhibitionSize.y );
    private static readonly Comparison<ImageInfo> SCREEN_HEIGHT_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.ExhibitionSize.y.CompareTo( a.Data.ExhibitionSize.y );
    private static readonly Comparison<ImageInfo> TEXTURE_SORT_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.Texture?.name.CompareTo( b.Data.Texture?.name ?? string.Empty ) ?? 1;
    private static readonly Comparison<ImageInfo> TEXTURE_SORT_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.Texture?.name.CompareTo( a.Data.Texture?.name ?? string.Empty ) ?? 1;
    private static readonly Comparison<ImageInfo> TEXTURE_WIDTH_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.TextureSize.x.CompareTo( b.Data.TextureSize.x );
    private static readonly Comparison<ImageInfo> TEXTURE_WIDTH_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.TextureSize.x.CompareTo( a.Data.TextureSize.x );
    private static readonly Comparison<ImageInfo> TEXTURE_HEIGHT_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.TextureSize.y.CompareTo( b.Data.TextureSize.y );
    private static readonly Comparison<ImageInfo> TEXTURE_HEIGHT_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.TextureSize.y.CompareTo( a.Data.TextureSize.y );
    private static readonly Comparison<ImageInfo> SCALE_X_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionScale.x.CompareTo( b.Data.ExhibitionScale.x );
    private static readonly Comparison<ImageInfo> SCALE_X_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.ExhibitionScale.x.CompareTo( a.Data.ExhibitionScale.x );
    private static readonly Comparison<ImageInfo> SCALE_Y_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.ExhibitionScale.y.CompareTo( b.Data.ExhibitionScale.y );
    private static readonly Comparison<ImageInfo> SCALE_Y_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.ExhibitionScale.y.CompareTo( a.Data.ExhibitionScale.y );
    private static readonly Comparison<ImageInfo> SCALE_MIN_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.MinScale.CompareTo( b.Data.MinScale );
    private static readonly Comparison<ImageInfo> SCALE_MIN_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.MinScale.CompareTo( a.Data.MinScale );
    private static readonly Comparison<ImageInfo> SCALE_MAX_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.MaxScale.CompareTo( b.Data.MaxScale );
    private static readonly Comparison<ImageInfo> SCALE_MAX_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.MaxScale.CompareTo( a.Data.MaxScale );
    private static readonly Comparison<ImageInfo> FILE_SIZE_ASC = ( ImageInfo a, ImageInfo b ) => a.Data.FileSize.CompareTo( b.Data.FileSize );
    private static readonly Comparison<ImageInfo> FILE_SIZE_DESC = ( ImageInfo a, ImageInfo b ) => b.Data.FileSize.CompareTo( a.Data.FileSize );

    private void SortByImage() { Sort( _lastComparison == IMAGE_SORT_ASC ? IMAGE_SORT_DESC : IMAGE_SORT_ASC ); }
    private void SortByScreenWidth() { Sort( _lastComparison == SCREEN_WIDTH_ASC ? SCREEN_WIDTH_DESC : SCREEN_WIDTH_ASC ); }
    private void SortByScreenHeight() { Sort( _lastComparison == SCREEN_HEIGHT_ASC ? SCREEN_HEIGHT_DESC : SCREEN_HEIGHT_ASC ); }
    private void SortByTexture() { Sort( _lastComparison == TEXTURE_SORT_ASC ? TEXTURE_SORT_DESC : TEXTURE_SORT_ASC ); }
    private void SortByTextureWidth() { Sort( _lastComparison == TEXTURE_WIDTH_ASC ? TEXTURE_WIDTH_DESC : TEXTURE_WIDTH_ASC ); }
    private void SortByTextureHeight() { Sort( _lastComparison == TEXTURE_HEIGHT_ASC ? TEXTURE_HEIGHT_DESC : TEXTURE_HEIGHT_ASC ); }
    private void SortByScaleX() { Sort( _lastComparison == SCALE_X_ASC ? SCALE_X_DESC : SCALE_X_ASC ); }
    private void SortByScaleY() { Sort( _lastComparison == SCALE_Y_ASC ? SCALE_Y_DESC : SCALE_Y_ASC ); }
    private void SortByMinScale() { Sort( _lastComparison == SCALE_MIN_ASC ? SCALE_MIN_DESC : SCALE_MIN_ASC ); }
    private void SortByMaxScale() { Sort( _lastComparison == SCALE_MAX_ASC ? SCALE_MAX_DESC : SCALE_MAX_ASC ); }
    private void SortBySize() { Sort( _lastComparison == FILE_SIZE_ASC ? FILE_SIZE_DESC : FILE_SIZE_ASC ); }

    public class ImageInfo
    {
        public readonly Image image;

        private AnalizedData cachedData;

        public class AnalizedData
        {
            public readonly Sprite Sprite;
            public readonly Texture2D Texture;
            public readonly Vector2 ExhibitionSize;
            public readonly Vector2 TextureSize;
            public readonly Vector2 ExhibitionScale;
            public readonly float MinScale;
            public readonly float MaxScale;
            public readonly long FileSize;

            public AnalizedData( Sprite sprite, Vector2 exhibitionSize )
            {
                Sprite = sprite;
                Texture = sprite != null ? sprite.texture : null;
                TextureSize = sprite != null ? new Vector2( sprite.rect.width, sprite.rect.height ) : Vector2.zero;
                ExhibitionSize = exhibitionSize;
                ExhibitionScale = new Vector2( TextureSize.x / ExhibitionSize.x, TextureSize.y / ExhibitionSize.y );
                MaxScale = Mathf.Max( ExhibitionScale.x, ExhibitionScale.y );
                MinScale = Mathf.Min( ExhibitionScale.x, ExhibitionScale.y );
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
                var cachedSize = cachedData?.ExhibitionSize ?? new Vector2( 1, 1 );
                if( cachedData == null || cachedData.Sprite != sprite || !Mathf.Approximately( eSize.x, cachedSize.x ) || !Mathf.Approximately( eSize.y, cachedSize.y )  ) cachedData = new AnalizedData( sprite, eSize );
                return cachedData;
            }
        }

        public ImageInfo( Image sr )
        {
            this.image = sr;
        }

        public void Draw( GUILayoutOption refOpt, GUILayoutOption dimensionW, GUILayoutOption textureOpt, GUILayoutOption fileSizeOpt )
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
            var eScale = data.ExhibitionScale;
            EditorGUILayout.LabelField( eScale.x.ToString( "N2" ), dimensionW );
            EditorGUILayout.LabelField( eScale.y.ToString( "N2" ), dimensionW );
            EditorGUILayout.LabelField( data.MinScale.ToString( "N2" ), dimensionW );
            EditorGUILayout.LabelField( data.MaxScale.ToString( "N2" ), dimensionW );
            EditorGUILayout.LabelField( data.FileSize.ToString( "N0" ), fileSizeOpt );
            EditorGUILayout.EndHorizontal();
        }
    }
}