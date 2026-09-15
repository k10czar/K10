using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class SubsetSelectorPredictorEditorExtensions
{

    public static float GetTableHeight<T>( this SubsetSelectorPredictor<T> predictor )
    {
        return ( predictor.Count + 1 ) *( EditorGUIUtility.singleLineHeight + 3 );
    }

    public static void DrawTable<T>( this SubsetSelectorPredictor<T> predictor, Rect rect )
    {
        var lh = EditorGUIUtility.singleLineHeight;

        int minE = int.MaxValue;
        int maxE = int.MinValue;

        var rMin = predictor.rMin;
        var rMax = predictor.rMax;
        var count = predictor.Count;
        var maxElementsCount = predictor.MaxElementsCount;

        var realSlices = 1;
        for( int j = 0; j <= maxElementsCount; j++ )
        {
            if( !predictor.HasChanceOfAnyElementCount(j) ) continue;
            realSlices++;
            minE = Mathf.Min( j, minE );
            maxE = Mathf.Max( j, maxE );
        }

        if( rMin <= rMax )
        {
            var toplineRect = rect.GetLineTop( lh + 3 );
            GUI.Label( toplineRect.VerticalSlice( 0, realSlices ), "Average", GUI.skin.box );
            var sliceId = 1;
            for( int j = minE; j <= maxE; j++ ) if( predictor.HasChanceOfAnyElementCount(j) ) GUI.Label( toplineRect.VerticalSlice( sliceId++, realSlices ), $"{j}", GUI.skin.box );

            for( int i = 0; i < count; i++ )
            {
                var lineRect = rect.GetLineTop( lh );
                sliceId = 1;
                GUI.Label( lineRect.VerticalSlice( 0, realSlices ), $"{predictor._elementAvg[i]:N2} {predictor._namesCache[i]}" );
                for( int j = minE; j <= maxE; j++ )
                {
                    if( !predictor.HasChanceOfAnyElementCount(j) ) continue;
                    double val = predictor._elementCountChance[ i, j ];
                    var r = lineRect.VerticalSlice( sliceId++, realSlices );
                    GUIProgressBar.Draw( r, (float)val );
                }
            }
        }
    }

    public static void DrawTable<T>( this SubsetSelectorPredictor<T> predictor )
    {
        var lh = EditorGUIUtility.singleLineHeight;

        int minE = int.MaxValue;
        int maxE = int.MinValue;

        var rMin = predictor.rMin;
        var rMax = predictor.rMax;
        var count = predictor.Count;
        var maxElementsCount = predictor.MaxElementsCount;

        var realSlices = 1;
        for( int j = 0; j <= maxElementsCount; j++ )
        {
            if( !predictor.HasChanceOfAnyElementCount(j) ) continue;
            realSlices++;
            minE = Mathf.Min( j, minE );
            maxE = Mathf.Max( j, maxE );
        }

        if( rMin <= rMax )
        {
            var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
            rect.RequestTop( lh + 3 );
            GUILayout.Space( lh + 3 );
            GUI.Label( rect.VerticalSlice( 0, realSlices ), "Average", GUI.skin.box );
            var sliceId = 1;
            for( int j = minE; j <= maxE; j++ ) if( predictor.HasChanceOfAnyElementCount(j) ) GUI.Label( rect.VerticalSlice( sliceId++, realSlices ), $"{j}", GUI.skin.box );
            EditorGUILayout.EndHorizontal();

            for( int i = 0; i < count; i++ )
            {
                rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
                rect.RequestTop( lh );
                GUILayout.Space( lh );
                GUI.Label( rect.VerticalSlice( 0, realSlices ), $"{predictor._elementAvg[i]:N2} {predictor._namesCache[i]}" );
                sliceId = 1;
                for( int j = minE; j <= maxE; j++ )
                {
                    if( !predictor.HasChanceOfAnyElementCount(j) ) continue;
                    double val = predictor._elementCountChance[ i, j ];
                    var r = rect.VerticalSlice( sliceId++, realSlices );
                    GUIProgressBar.Draw( r, (float)val );
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public static float GetPermutationsHeight<T>( this SubsetSelectorPredictor<T> predictor )
    {
        return EditorGUIUtility.singleLineHeight * predictor.PermutationsCount;
    }

    public static void DrawPermutations<T>( this SubsetSelectorPredictor<T> predictor, Rect rect )
    {
        var maxLabelSize = 24f;
        GUIStyle labelStyle = GUI.skin.label;
        foreach(var permutation in predictor.PermutationsList ) 
        {
            var size = labelStyle.CalcSize(new GUIContent(permutation.name)).x;
            if( maxLabelSize > size ) continue;
            maxLabelSize = size;
        }
        var labelHeight = EditorGUIUtility.singleLineHeight;

        if( maxLabelSize > rect.width - 32 ) maxLabelSize = rect.width - 32;

        foreach(var permutation in predictor.PermutationsList )
        {
            var line = rect.GetLineTop( labelHeight );
            GUI.Label( line.RequestLeft( maxLabelSize ), permutation.name, labelStyle );
            GUIProgressBar.Draw( line.CutLeft( maxLabelSize ), permutation.percentage );
        }
    }

    public static void DrawPermutations<T>( this SubsetSelectorPredictor<T> predictor )
    {
        EditorGUILayout.BeginVertical();
        var maxSize = 24f;
        GUIStyle labelStyle = GUI.skin.label;
        foreach(var permutation in predictor.PermutationsList ) 
        {
            var size = labelStyle.CalcSize(new GUIContent(permutation.name)).x;
            if( maxSize > size ) continue;
            maxSize = size;
        }
        var labelWidth = GUILayout.Width(maxSize);
        var labelHeight = GUILayout.Height( EditorGUIUtility.singleLineHeight );

        foreach(var permutation in predictor.PermutationsList )
        {
            EditorGUILayout.BeginHorizontal( labelHeight );
            GUILayout.Label( permutation.name, labelStyle, labelWidth, labelHeight );
            GUIProgressBar.DrawLayout( permutation.percentage, GUILayout.MinWidth( 32 ) );
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}

