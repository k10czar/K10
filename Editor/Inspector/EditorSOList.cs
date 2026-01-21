using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorSOList<T> where T : ScriptableObject
{
    public const float DEFAULT_MAX_HEIGHT = 220;
    const float MARGIN = 3;

    List<T> queriedRefs = new();
    public readonly Color BaseColor = Colors.Gold;
    public readonly float MaxHeight = DEFAULT_MAX_HEIGHT;
    bool _readonly = true;
    private Vector2 _scroll;

    public int Count => queriedRefs.Count;

    public void Clear() => queriedRefs.Clear();
    public void Add( T t ) => queriedRefs.Add( t );
    public void AddRange( IEnumerable<T> range ) => queriedRefs.AddRange( range );

    public EditorSOList( float maxHeight = DEFAULT_MAX_HEIGHT ) : this( Colors.Gold, maxHeight ) { }
    public EditorSOList( Color color, float maxHeight = DEFAULT_MAX_HEIGHT )
    {
        MaxHeight = maxHeight;
        BaseColor = color;
    }

    public void DrawLayout()
    {
        _scroll = EditorGUILayout.BeginScrollView( _scroll, GUILayout.Height( GetHeight() ) );
        var referencesCount = queriedRefs.Count;
        if( referencesCount == 0 ) EditorGUILayout.LabelField( $"empty", K10GuiStyles.smallStyle );
        else 
        {
            using (new EditorGUI.DisabledScope(_readonly)) 
                DrawListLayout();
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawListLayout()
    {
        var referencesCount = queriedRefs.Count;
        var numWidth = GUILayout.Width( 5 + 10 * ( K10.Math.Log10( referencesCount ) + 1 ) );
        for (int i = 0; i < referencesCount; i++)
        {
            T e = queriedRefs[i];
            GuiColorManager.New( BaseColor.WithSaturation( .4f ).WithValue(i%2==1?.1f:.2f).WithAlpha( .5f ) );
            EditorGUILayout.BeginHorizontal( K10GuiStyles.whiteBackgroundStyle );
            GuiColorManager.Revert();
            EditorGUILayout.LabelField( $"{i}]", K10GuiStyles.smallboldRightStyle, numWidth );
            EditorGUILayout.ObjectField( e, typeof( T ), false );
            EditorGUILayout.EndHorizontal();
        }
    }

    public float GetHeight()
    {
        var slh = EditorGUIUtility.singleLineHeight;
        var lines = queriedRefs?.Count ?? 1;
        if( lines == 0 ) lines = 1;
        var lh = slh + 3;
        var linesHeight = lines * lh;
        if( linesHeight > MaxHeight ) return MaxHeight;
        return linesHeight + 2 * MARGIN;
    }
}
