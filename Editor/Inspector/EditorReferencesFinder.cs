using System;
using System.Collections.Generic;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

public class EditorReferencesFinder : EditorReferencesFinder<ScriptableObject>
{
    public EditorReferencesFinder( Func<IEnumerable<ScriptableObject>> finderFunc, Color color, float maxHeight = DEFAULT_MAX_HEIGHT ) : base( finderFunc, color, maxHeight ) { }
    public EditorReferencesFinder( Func<IEnumerable<ScriptableObject>> finderFunc, float maxHeight = DEFAULT_MAX_HEIGHT ) : base( finderFunc, Colors.Gold, maxHeight ) { }
}

public class EditorReferencesFinder<T> where T : ScriptableObject
{
    public const float DEFAULT_MAX_HEIGHT = 240;

    static IValueCapsule<bool> openInspection = new LazyEditorPersistentValue<bool>( "RefInspect" );
    Func<IEnumerable<T>> FinderFunc;
    private double _inspectionTime = 0;

    EditorSOList<T> _references = new();

    public EditorReferencesFinder( Func<IEnumerable<T>> finderFunc, Color color, float maxHeight = DEFAULT_MAX_HEIGHT )
    {
        _references = new( color, maxHeight );
        FinderFunc = finderFunc;
    }

    public EditorReferencesFinder( Func<IEnumerable<T>> finderFunc, float maxHeight = DEFAULT_MAX_HEIGHT ) : this( finderFunc, Colors.Gold, maxHeight ) { }

    public void Query()
    {
        if( FinderFunc == null ) return;
        var sw = StopwatchPool.RequestStarted();
        _references.Clear();
        var refs = FinderFunc();
        _references.AddRange( refs );
        _inspectionTime = sw.ReturnToPoolAndGetElapsedMs();
    }

    public void TryQuery()
    {
        if( !openInspection.Get ) return;
        Query();
    }

    public void TryQuery( Func<IEnumerable<T>> newFinderFunc )
    {
        FinderFunc = newFinderFunc;
        TryQuery();
    }

    public void DrawLayout()
    {
		GuiColorManager.New( _references.BaseColor.WithSaturation( .4f ).WithValue(.5f).WithAlpha( .5f ) );
        EditorGUILayout.BeginVertical( K10GuiStyles.whiteBackgroundStyle );
        GuiColorManager.Revert();
        var inspect = openInspection;
        var open = EditorGUILayout.BeginFoldoutHeaderGroup( inspect.Get, "🕵️ Reference Inspector", K10GuiStyles.bigFoldStyle );
        inspect.Set = open;
        if( open )
        {
		    GuiColorManager.New( _references.BaseColor.WithSaturation( .4f ).WithValue(.3f).WithAlpha( .5f ) );
            EditorGUILayout.BeginHorizontal( K10GuiStyles.whiteBackgroundStyle );
            GuiColorManager.Revert();
            IconButton.Layout( UnityIcons.Refresh );
            EditorGUILayout.LabelField( $"Found {_references.Count} references in ⏳ {_inspectionTime}ms", K10GuiStyles.smallStyle );
            EditorGUILayout.EndHorizontal();
            _references.DrawLayout();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndVertical();
    }

    public float GetHeight()
    {
        var slh = EditorGUIUtility.singleLineHeight;
        if( !openInspection.Get ) return slh;
        return 2 * slh + _references.GetHeight();
    }
}
