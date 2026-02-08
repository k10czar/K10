using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PreditionsDrawer
{
	public enum ERangeSort
	{
		MinAscending,
		MinDescending,
		AverageAscending,
		AverageDescending,
		MaxAscending,
		MaxDescending,
	}

    static IValueCapsule<ERangeSort> rangeSort = new LazyEditorPersistentValue<ERangeSort>( "PredictionsRangeSort", ERangeSort.AverageDescending );

	public static void DrawTableLayout<T>( this BaseAggregatedPredictor<T> aggregated )
	{
		var lh = EditorGUIUtility.singleLineHeight;

		int minE = int.MaxValue;
		int maxE = int.MinValue;
		var count = aggregated._elementsCount;
		var maxElementsCount = aggregated._maxElementCount;

		var realSlices = 2;
		for( int j = 0; j <= maxElementsCount; j++ )
		{
			if( !aggregated.HasChanceOfAnyElementCount(j) ) continue;
			realSlices++;
			minE = Mathf.Min( j, minE );
			maxE = Mathf.Max( j, maxE );
		}

		// GUILayout.Label( $"[ {minE}, {maxE} ] ( {realSlices} ) in [ {0}, {maxElementsCount} ]", GUI.skin.box );
		// GUILayout.Label( aggregated.ToStringOrNull(), GUI.skin.box );

		if( minE <= maxE )
		{
			var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
			rect.RequestTop( lh + 3 );
			GUILayout.Space( lh + 3 );
			GUI.Label( rect.VerticalSlice( 0, realSlices ), "Element", GUI.skin.box );
			GUI.Label( rect.VerticalSlice( 1, realSlices ), "Average", GUI.skin.box );
			var sliceId = 2;
			for( int j = minE; j <= maxE; j++ ) if( aggregated.HasChanceOfAnyElementCount(j) ) GUI.Label( rect.VerticalSlice( sliceId++, realSlices ), $"{j}", GUI.skin.box );
			EditorGUILayout.EndHorizontal();

			for( int i = 0; i < count; i++ )
			{
				rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
				rect.RequestTop( lh );
				GUILayout.Space( lh );
				GUI.Label( rect.VerticalSlice( 0, realSlices ), aggregated._namesCache[i], K10GuiStyles.boldStyle );
				GUI.Label( rect.VerticalSlice( 1, realSlices ), aggregated._elementAvg[i].ToString( "N1" ), K10GuiStyles.boldCenterStyle );
				sliceId = 2;
				for( int j = minE; j <= maxE; j++ )
				{
					if( !aggregated.HasChanceOfAnyElementCount(j) ) continue;
					double val = aggregated._elementCountChance[ i, j ];
					var r = rect.VerticalSlice( sliceId++, realSlices );
					GUIProgressBar.Draw( r, (float)val );
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		// for( int i = 0; i < aggregated.SubPredictorCount; i++ )
		// {
		// 	var subPredictor = aggregated.GetSubPredictor(i);
		// 	subPredictor.DrawTableLayout();
		// }
	}

	// public static void DrawElementsRanges( this ICompoundAggregatedPredictor predictor )
	public static void DrawElementsRanges<T,K>( this CompoundAggregatedPredictor<T,K> compound ) where T : ScriptableObject, IAggregatedSubsetSelector<K>
	{
		var elements = compound.GetSubElementAveragesEnumerator();

		var style = K10GuiStyles.boldRightStyle;
		// var maxWidth = 132f;
		var maxWidth = 30f;

		var sortedElements = new List<KeyValuePair<K,RangeSummary>>();
		while( elements.MoveNext() ) 
		{
			var element = elements.Current;
			sortedElements.Add( element );
			var size = style.CalcSize( new GUIContent( element.Value.ToStringValues() ) );
			if( size.x > maxWidth ) maxWidth = size.x;
		}
		
		var widthLayoutProp = GUILayout.Width( maxWidth );
		var sortWidthProp = GUILayout.Width( maxWidth / 3 - 2 );
		var sortHeightProp = GUILayout.Height( 18 );
		
		EditorGUILayout.BeginVertical( GUI.skin.box );

		EditorGUILayout.BeginHorizontal( sortHeightProp );
		SortButton( "min", ERangeSort.MinAscending, sortWidthProp, sortHeightProp );
		SortButton( "avg", ERangeSort.AverageAscending, sortWidthProp, sortHeightProp );
		SortButton( "max", ERangeSort.MaxAscending, sortWidthProp, sortHeightProp );
		GUILayout.Label( $"Count Range of {typeof(T).Name}" );
		EditorGUILayout.EndHorizontal();

        switch (rangeSort.Get)
        {
            case ERangeSort.MinAscending: sortedElements.Sort( MinAscending ); break;
            case ERangeSort.MinDescending: sortedElements.Sort( MinDescending ); break;
            case ERangeSort.AverageAscending: sortedElements.Sort( AverageAscending ); break;
            case ERangeSort.AverageDescending: sortedElements.Sort( AverageDescending ); break;
            case ERangeSort.MaxAscending: sortedElements.Sort( MaxAscending ); break;
            case ERangeSort.MaxDescending: sortedElements.Sort( MaxDescending ); break;
        }

        foreach ( var element in sortedElements )
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( element.Value.ToStringValues(), style, widthLayoutProp );
			EditorGUILayout.ObjectField( element.Key as ScriptableObject, typeof(K), false );
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndVertical();
    }

    private static int MinAscending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByMin( x.Value, y.Value );
    private static int MinDescending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByDescendingMin( x.Value, y.Value );
    private static int AverageAscending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByAverage( x.Value, y.Value );
    private static int AverageDescending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByDescendingAverage( x.Value, y.Value );
    private static int MaxAscending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByMax( x.Value, y.Value );
    private static int MaxDescending<T>(KeyValuePair<T, RangeSummary> x, KeyValuePair<T, RangeSummary> y) => RangeSummary.ByDescendingMax( x.Value, y.Value );

    static void SortButton( string label, ERangeSort state0, params GUILayoutOption[] options ) { SortButton( label, state0, state0 + 1, options ); }
	static void SortButton( string label, ERangeSort state0, ERangeSort state1, params GUILayoutOption[] options )
	{
		var sortType = rangeSort.Get;
		var isSort0 = sortType == state0;
		var isSort1 = sortType == state1;
		var tinted = isSort0 || isSort1;
		if( isSort0 ) GuiColorManager.New( Colors.DarkOrange.WithValue( .95f ) );
		if( isSort1 ) GuiColorManager.New( Colors.Volt.WithValue( .95f ) );
		// if( GUILayout.Button( ( isSort0 ? "⬆" : isSort1 ? "⬇" : "" ) + label, options ) ) rangeSort.Set = isSort0 ? state1 : state0;
		if( GUILayout.Button( label, options ) ) rangeSort.Set = isSort0 ? state1 : state0;
		if( tinted ) GuiColorManager.Revert();
	}

	public static void DrawTableLayout<T>( this BaseSubsetSelectorPredictor<T> subset )
	{
		var lh = EditorGUIUtility.singleLineHeight;

		int minE = int.MaxValue;
		int maxE = int.MinValue;
		var count = subset.Count;
		var maxElementsCount = subset.MaxElementsCount;
		
		var realSlices = 2;
		for( int j = 0; j <= maxElementsCount; j++ )
		{
			if( !subset.HasChanceOfAnyElementCount(j) ) continue;
			realSlices++;
			minE = Mathf.Min( j, minE );
			maxE = Mathf.Max( j, maxE );
		}

		if( minE <= maxE )
		{
			var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
			rect.RequestTop( lh + 3 );
			GUILayout.Space( lh + 3 );
			GUI.Label( rect.VerticalSlice( 0, realSlices ), "Element", GUI.skin.box );
			GUI.Label( rect.VerticalSlice( 1, realSlices ), "Average", GUI.skin.box );
			var sliceId = 2;
			for( int j = minE; j <= maxE; j++ ) if( subset.HasChanceOfAnyElementCount(j) ) GUI.Label( rect.VerticalSlice( sliceId++, realSlices ), $"{j}", GUI.skin.box );
			EditorGUILayout.EndHorizontal();

			for( int i = 0; i < count; i++ )
			{
				rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
				rect.RequestTop( lh );
				GUILayout.Space( lh );
				GUI.Label( rect.VerticalSlice( 0, realSlices ), subset._namesCache[i], K10GuiStyles.boldRightStyle );
				GUI.Label( rect.VerticalSlice( 1, realSlices ), subset._elementAvg[i].ToString( "N1" ), K10GuiStyles.boldCenterStyle );
				sliceId = 2;
				for( int j = minE; j <= maxE; j++ )
				{
					if( !subset.HasChanceOfAnyElementCount(j) ) continue;
					double val = subset._elementCountChance[ i, j ];
					var r = rect.VerticalSlice( sliceId++, realSlices );
					GUIProgressBar.Draw( r, (float)val );
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
