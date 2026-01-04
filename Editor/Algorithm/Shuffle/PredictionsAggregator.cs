using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class PreditionsDrawer
{
	const float SPACING = 3;

	static ToggleButtonFromPropExpansionLazy _showPreditions = new( "Chances Prediction", "icons/dices.png", null );
	// static PersistentToggleButton _showPreditions = new( "showPredictions", "Chances Prediction", "icons/dices.png", null, false );
	static PersistentToggleButton _showCountingTable = new( "showCountTable", "Count Chances", UnityIcons._Menu, null, false );
	static PersistentToggleButton _showPermutations = new( "showPermutations", "Permutations", UnityIcons.d_SortingGroup_Icon, UnityIcons.SortingGroup_Icon, false );

    public static void DrawLayout( this PredictionsAggregator aggregated )
    {
		EditorGUILayout.BeginVertical( GUI.skin.box );
			if( !_showPreditions.Layout( aggregated.Prop ) )
			{
				GUILayout.Space( SPACING );
				var count = aggregated.Count;
				if( _showCountingTable.Layout() )
				{
					EditorGUILayout.BeginHorizontal( GUI.skin.box );
						for( int i = 0; i < count; i++ )
						{
							GUILayout.Space( SPACING );
							EditorGUILayout.BeginVertical( GUI.skin.box );
								var predictor = aggregated.GetEntry( i );
								predictor.DrawTable();
							EditorGUILayout.EndVertical();
						}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.Space( SPACING );
				if( _showPermutations.Layout() )
				{
					EditorGUILayout.BeginHorizontal( GUI.skin.box );
						for( int i = 0; i < count; i++ )
						{
							GUILayout.Space( SPACING );
							EditorGUILayout.BeginVertical( GUI.skin.box );
								var predictor = aggregated.GetEntry( i );
								predictor.DrawPermutations();
							EditorGUILayout.EndVertical();
						}
					EditorGUILayout.EndHorizontal();
				}
			}
		EditorGUILayout.EndVertical();
    }
	

    public static void Draw( this PredictionsAggregator aggregated, Rect rect)
    {
		if( _showPreditions.DrawOnTop( aggregated.Prop, ref rect ) )
        {
			var count = aggregated.Count;

            rect.GetLineTop( SPACING );

			if( _showCountingTable.DrawOnTop( ref rect ) )
            {
				var height = MaxCountTableHeight( aggregated );
				var area = rect.GetLineTop( height );
				for( int i = 0; i < count; i++ )
				{
					var predictor = aggregated.GetEntry( i );
					var h = predictor.GetTableHeight();
					var slicedArea = area.VerticalSlice( i, count ).RequestTop( h );
					GUI.Box( slicedArea, GUIContent.none );
					predictor.DrawTable( slicedArea );
				}
            }

            rect.GetLineTop( SPACING );

			if( _showPermutations.DrawOnTop( ref rect ) )
            {
				var height = MaxPermutationHeight( aggregated );
				var area = rect.GetLineTop( height );
				for( int i = 0; i < count; i++ )
				{
					var predictor = aggregated.GetEntry( i );
					var h = predictor.GetPermutationsHeight();
					var slicedArea = area.VerticalSlice( i, count ).RequestTop( h );
					GUI.Box( slicedArea, GUIContent.none );
					predictor.DrawPermutations( slicedArea );
				}
            }
        }
    }

	static float MaxCountTableHeight( PredictionsAggregator aggregated )
    {
		var count = aggregated.Count;
		var maxCountTableHeight = 0f;
		if( _showCountingTable.Enabled )
		{
			for( int i = 0; i < count; i++ )
			{
				var h = aggregated.GetEntry( i ).GetTableHeight();
				if( maxCountTableHeight < h ) maxCountTableHeight = h;
			}
		} 
		return maxCountTableHeight + SPACING;
    }

	static float MaxPermutationHeight( PredictionsAggregator aggregated )
    {
		var count = aggregated.Count;
		var maxPermutationHeight = 0f;
		if( _showPermutations.Enabled ) 
		{
			for( int i = 0; i < count; i++ )
			{
				var h = aggregated.GetEntry( i ).GetPermutationsHeight();
				if( maxPermutationHeight < h ) maxPermutationHeight = h;
				// height += h + SPACING;
			}
		}
		return maxPermutationHeight + SPACING;
    }

    public static float GetHeight( this PredictionsAggregator aggregated )
    {
		var height = _showPreditions.GetHeight( aggregated.Prop );
		if( aggregated.Prop?.isExpanded ?? false ) 
		{
			height += _showCountingTable.GetHeight() + _showPermutations.GetHeight() + 2 * SPACING;

			var count = aggregated.Count;
			if( _showCountingTable.Enabled )
            {
				// for( int i = 0; i < count; i++ )
                // {
				// 	var h = aggregated.GetEntry( i ).GetTableHeight();
				// 	height += h + SPACING;   
                // }
				height += MaxCountTableHeight( aggregated ) + SPACING;
            } 
			if( _showPermutations.Enabled ) 
            {
				// for( int i = 0; i < count; i++ )
                // {
				// 	var h = aggregated.GetEntry( i ).GetPermutationsHeight();
				// 	height += h + SPACING;
                // }
				height += MaxPermutationHeight( aggregated ) + SPACING;
			}
		}
		return height;
    }
}

public class PredictionsAggregator
{
	List<ChancesPredictor> _predictors = new();
	
	// int[] _countCache;
	// string[] _namesCache;
	// int[] _countStarts;

	int _count;

	SerializedProperty _prop;

	public int Count => _count;

    public SerializedProperty Prop => _prop;

    // List<(string name, float percentage)> _permutations = new();
	// Dictionary<string, float> _permutationChances = new();

	public ChancesPredictor GetEntry( int index ) => _predictors[index];

	public void EnsureSize( int size )
	{
		while( _predictors.Count < size )
		{
			_predictors.Add( new() );
		}
	}

	public void SetProp( SerializedProperty prop )
	{
		_prop = prop;
	}

	public void SetDirty()
	{
		
	}

	// public void DrawPermutations()
	// {
	// 	EditorGUILayout.BeginVertical();
	// 	var maxSize = 24f;
	// 	GUIStyle labelStyle = GUI.skin.label;
	// 	foreach(var permutation in _permutations ) maxSize = MathAdapter.max( maxSize, labelStyle.CalcSize(new GUIContent(permutation.name)).x );
	// 	var labelWidth = GUILayout.Width(maxSize);
	// 	var labelHeight = GUILayout.Height( EditorGUIUtility.singleLineHeight );

	// 	foreach(var permutation in _permutations )
	// 	{
	// 		EditorGUILayout.BeginHorizontal( labelHeight );
	// 		GUILayout.Label( permutation.name, labelStyle, labelWidth, labelHeight );
	// 		GUIProgressBar.DrawLayout( permutation.percentage, GUILayout.MinWidth( 32 ) );
	// 		EditorGUILayout.EndHorizontal();
	// 	}
	// 	EditorGUILayout.EndVertical();
	// }

	// private int PermutationSorting((string, float) x, (string, float) y) => y.Item2.CompareTo( x.Item2 );

	// void Permutations()
	// {
	// 	_permutations.Clear();
	// 	_permutationChances.Clear();

	// 	_count = _predictors.Count;
	// 	if( _countStarts == null || _countStarts.Length < _count ) _countStarts = new int[_count];

	// 	var poolCount = 0;
	// 	for( int i = 0; i < _count; i++)
	// 	{
	// 		var p = _predictors[i];
	// 		var pCount = p._minCache.Length;
	// 		poolCount += pCount;
	// 		_countStarts[i] = pCount;
	// 	}
		
	// 	if( _countCache == null || _countCache.Length < poolCount ) _countCache = new int[poolCount];

	// 	// Permute( 0, 0 );
	// }

	// void Permute( int permIt, int elementIt )
	// {
	// 	// if( permIt >= _count )
	// 	// {
	// 	// 	var name = NameCombination(_countCache, _namesCache, it);
	// 	// 	_permutationChances.TryGetValue(name, out var chance);
	// 	// 	_permutationChances[name] = chance + currChance;
	// 	// 	return;
	// 	// }

	// 	for( int i = 0; i < _count; i++)
	// 	{
	// 		var p = _predictors[i];

	// 		var rollsCount = p.rMax + 1 - p.rMin;
	// 		var defaultChance = 1f / rollsCount;
	// 		var rChanLen = p._rollChancesCache?.Length ?? 0;
	// 		if( rChanLen > rollsCount ) rChanLen = rollsCount;
	// 		var biased = rChanLen > 0;

	// 		var baseMins = 0;
	// 		for (int j = 0; j < p._minCache.Length; j++)
	// 		{
	// 			baseMins += p._minCache[j];
	// 			p._countCache[j] = p._minCache[j];
	// 		}

	// 		var rwSum = 0f;
	// 		for (int j = 0; j < rChanLen; j++) rwSum += p._rollChancesCache[j];

	// 		for( int j = 0; j < rollsCount; j++ )
	// 		{
	// 			var rolls = j + p.rMin;
	// 			var chance = defaultChance;
	// 			if( biased ) chance = p._rollChancesCache[ Mathf.Min( j, rChanLen - 1 )] / rwSum;
	// 			// Permute(baseMins, rolls, chance );
	// 		}

	// 		foreach( var e in _permutationChances ) _permutations.Add( ( e.Key, e.Value ) );
	// 	}
	// }
	
	// void Permute( int it, int maxPermute, float currChance, ChancesPredictor pre )
	// {
	// 	if( it >= maxPermute )
	// 	{
	// 		var name = NameCombination(pre._countCache, pre._namesCache, it);
	// 		_permutationChances.TryGetValue(name, out var chance);
	// 		_permutationChances[name] = chance + currChance;
	// 		return;
	// 	}
	// 	var sumWeights = 0f;

	// 	var count = pre._maxCache.Length;
	// 	for( int k = 0; k < count; k++ )
	// 	{
	// 		if (pre._maxCache[k] <= pre._countCache[k]) continue;
	// 		var eChan = pre._chancesCache[k];
	// 		sumWeights += eChan;
	// 	}
		
	// 	for( int k = 0; k < count; k++ )
	// 	{
	// 		if (pre._maxCache[k] <= pre._countCache[k]) continue;
	// 		var eChan = pre._chancesCache[k];
	// 		pre._countCache[k]++;
	// 		Permute( it + 1, maxPermute, currChance * ( eChan / sumWeights ), pre );
	// 		pre._countCache[k]--;
	// 	}
	// }

	// private string NameCombination( int[] _countCache, string[] names, int maxRolls = int.MaxValue)
	// {
	// 	var sb = StringBuilderPool.RequestEmpty();

	// 	var first = true;
	// 	for (int i = 0; i < _countCache.Length; i++)
	// 	{
	// 		var count = Mathf.Min( _countCache[i], maxRolls );
	// 		if (count > 0)
	// 		{
	// 			if( !first ) sb.Append( ", " );
	// 			sb.Append($"{count} {names[i]}");
	// 			first = false;
	// 		}
	// 		maxRolls -= count;
	// 	}

	// 	if( first ) sb.Append( "NOTHING" );

	// 	return sb.ReturnToPoolAndCast();
	// }

    public void Calculate()
    {
		_count = _prop.arraySize;
		EnsureSize(_count);

		for( int i = 0; i < _count; i++ )
        {
            var entry = _prop.GetArrayElementAtIndex(i);
			SetSpecificData( i, entry );
        }
    }

	public void SetSpecificData(int i, SerializedProperty entry)
	{
		_predictors[i].Calculate( entry );
	}
}
