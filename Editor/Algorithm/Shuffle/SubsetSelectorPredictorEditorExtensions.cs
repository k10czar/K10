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

        for( int k = rMin; k <= rMax; k++ )
        {
            for( int i = 0; i < count; i++ )
            {
                for( int j = 0; j <= predictor.MaxElementsCount; j++ )
                {
                    if( Mathf.Approximately( (float)predictor.M[i, j, k], 0 ) ) continue;
                    minE = Mathf.Min( j, minE );
                    maxE = Mathf.Max( j, maxE );
                }
            }
        }

        var slices = ( maxE - minE ) + 2;
        if( rMin <= rMax )
        {
            var toplineRect = rect.GetLineTop( lh + 3 );
            GUI.Label( toplineRect.VerticalSlice( 0, slices ), "Average", GUI.skin.box );
            for( int j = minE; j <= maxE; j++ ) GUI.Label( toplineRect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
            
            var range = Mathf.Max( rMax - rMin, 0 ) + 1;
            float sumWeight = range;
            
            var _rollChancesCache = predictor._rollChancesCache;
            if (_rollChancesCache != null && _rollChancesCache.Length >= range)
            {
                sumWeight = 0;
                for (int i = 0; i < range; i++)
                {
                    sumWeight += _rollChancesCache[i];
                }
            }

            for( int i = 0; i < count; i++ )
            {
                var lineRect = rect.GetLineTop( lh );
                GUI.Label( lineRect.VerticalSlice( 0, slices ), predictor._namesCache[i] );
                for( int j = minE; j <= maxE; j++ )
                {
                    double val = 0;
                    for (int k = rMin; k <= rMax; k++)
                    {
                        var chance = predictor.M[i, j, k];
                        if (_rollChancesCache != null && k - rMin < _rollChancesCache.Length) chance *= _rollChancesCache[k - rMin];
                        val += chance;
                    }
                    val /= sumWeight;
                    val = System.Math.Max( val, 0 );

                    var r = lineRect.VerticalSlice( ( j - minE ) + 1, slices );
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

        for( int k = rMin; k <= rMax; k++ )
        {
            for( int i = 0; i < count; i++ )
            {
                for( int j = 0; j <= maxElementsCount; j++ )
                {
                    if( Mathf.Approximately( (float)predictor.M[i, j, k], 0 ) ) continue;
                    minE = Mathf.Min( j, minE );
                    maxE = Mathf.Max( j, maxE );
                }
            }
        }

        var slices = ( maxE - minE ) + 2;
        if( rMin <= rMax )
        {
            var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
            rect.RequestTop( lh + 3 );
            GUILayout.Space( lh + 3 );
            GUI.Label( rect.VerticalSlice( 0, slices ), "Average", GUI.skin.box );
            for( int j = minE; j <= maxE; j++ ) GUI.Label( rect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
            EditorGUILayout.EndHorizontal();
            
            var range = Mathf.Max( rMax - rMin, 0 ) + 1;
            float sumWeight = range;
            
            var _rollChancesCache = predictor._rollChancesCache;
            if (_rollChancesCache != null && _rollChancesCache.Length >= range)
            {
                sumWeight = 0;
                for (int i = 0; i < range; i++)
                {
                    sumWeight += _rollChancesCache[i];
                }
            }

            for( int i = 0; i < count; i++ )
            {
                rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
                rect.RequestTop( lh );
                GUILayout.Space( lh );
                GUI.Label( rect.VerticalSlice( 0, slices ), predictor._namesCache[i] );
                for( int j = minE; j <= maxE; j++ )
                {
                    double val = 0;
                    for (int k = rMin; k <= rMax; k++)
                    {
                        var chance = predictor.M[i, j, k];
                        if (_rollChancesCache != null && k - rMin < _rollChancesCache.Length) chance *= _rollChancesCache[k - rMin];
                        val += chance;
                    }
                    val /= sumWeight;
                    val = System.Math.Max( val, 0 );

                    var r = rect.VerticalSlice( ( j - minE ) + 1, slices );
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
    
    // public static void Calculate(SerializedProperty prop)
    // {
	// 	var entries = prop.FindPropertyRelative("_entries");
	// 	var rolls = prop.FindPropertyRelative("_rolls");
	// 	FillCaches( entries, rolls.FindPropertyRelative("_weights"));
	// 	var range = rolls.FindPropertyRelative("range");
	// 	var min = range.FindPropertyRelative("min").intValue;
	// 	var max = range.FindPropertyRelative("max").intValue;
	// 	count = entries.arraySize;

    //     CalculatePredictions( min, max, entries );
    // }

    // public static void FillCaches( SerializedProperty entriesProp, SerializedProperty rangeWeightsProp )
	// {
	// 	FillCaches( entriesProp );

	// 	var rCount = rangeWeightsProp.arraySize;
	// 	if( _rollChancesCache == null || _rollChancesCache.Length != rCount ) _rollChancesCache = new float[rCount];

	// 	for (int i = 0; i < rCount; i++)
	// 	{
	// 		var element = rangeWeightsProp.GetArrayElementAtIndex(i);
	// 		_rollChancesCache[i] = element.floatValue;
	// 	}
    // }

	// public static void FillCaches( SerializedProperty entriesProp )
	// {
	// 	var count = entriesProp.arraySize;

	// 	if (_minCache == null || _minCache.Length != count) _minCache = new int[count];
	// 	if (_maxCache == null || _maxCache.Length != count) _maxCache = new int[count];
	// 	if (_namesCache == null || _namesCache.Length != count) _namesCache = new string[count];
	// 	if (_chancesCache == null || _chancesCache.Length != count) _chancesCache = new float[count];

	// 	_sumWeight = 0f;

	// 	for( int i = 0; i < count; i++ )
	// 	{
	// 		var element = entriesProp.GetArrayElementAtIndex( i );
	// 		_sumWeight += element.FindPropertyRelative( "_weight" ).floatValue;
	// 	}

	// 	minSum = 0;
	// 	maxElementsCount = 0;

	// 	for (int i = 0; i < count; i++)
	// 	{
	// 		var element = entriesProp.GetArrayElementAtIndex(i);
	// 		_chancesCache[i] = element.FindPropertyRelative("_weight").floatValue / _sumWeight;
	// 		var min = element.FindPropertyRelative("_guaranteed").intValue;
	// 		_minCache[i] = min;
	// 		var max =  element.FindPropertyRelative("_cap").intValue;
	// 		_maxCache[i] = max;
	// 		var objRef = element.FindPropertyRelative("_element").objectReferenceValue;
	// 		_namesCache[i] = objRef.DebugNameOrNull();
	// 		minSum += min;
			
	// 		var realCap = Mathf.Max( min, max );
	// 		maxElementsCount = Mathf.Max( maxElementsCount, realCap );
	// 	}

	// 	_rollChancesCache = null;
    // }

	// public static void CalculatePredictions( int minRoll, int maxRoll, SerializedProperty entriesProp )
	// {
	// 	int count = entriesProp.arraySize;

	// 	float sumWeight = 0f;

	// 	for( int i = 0; i < count; i++ )
	// 	{
	// 		var element = entriesProp.GetArrayElementAtIndex( i );
	// 		sumWeight += element.FindPropertyRelative( "_weight" ).floatValue;
	// 	}

	// 	CalculatePredictions( minRoll, maxRoll, sumWeight );
	// }
}

