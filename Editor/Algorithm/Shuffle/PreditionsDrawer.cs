using UnityEditor;
using UnityEngine;

public static class PreditionsDrawer
{
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
				GUI.Label( rect.VerticalSlice( 1, realSlices ), aggregated._elementAvg[i].ToString( "N2" ), K10GuiStyles.boldCenterStyle );
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

		while( elements.MoveNext() )
		{
			var element = elements.Current;
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( element.Value.ToString(), K10GuiStyles.boldRightStyle, GUILayout.Width( 500 ) );
			EditorGUILayout.ObjectField( element.Key as ScriptableObject, typeof(K), false );
			EditorGUILayout.EndHorizontal();
		}
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
				GUI.Label( rect.VerticalSlice( 1, realSlices ), subset._elementAvg[i].ToString( "N2" ), K10GuiStyles.boldCenterStyle );
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
