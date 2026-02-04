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
								if( predictor != null ) predictor.DrawTable();
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
								if( predictor != null ) predictor.DrawPermutations();
							EditorGUILayout.EndVertical();
						}
					EditorGUILayout.EndHorizontal();
				}
			}
		EditorGUILayout.EndVertical();
    }

	public static void DrawTableLayout<T>( this AggregatedPredictor<T> aggregated )
	{
        var lh = EditorGUIUtility.singleLineHeight;
		

        int minE = int.MaxValue;
        int maxE = int.MinValue;
        var count = aggregated._elementsCount;
        var maxElementsCount = aggregated._maxElementCount;

        var realSlices = 1;
        for( int j = 0; j <= maxElementsCount; j++ )
        {
            if( !aggregated.HasChanceOfAnyElementCount(j) ) continue;
            realSlices++;
            minE = Mathf.Min( j, minE );
            maxE = Mathf.Max( j, maxE );
        }

        GUILayout.Label( $"Averageas da sd [ {minE}, {maxE} ] {maxElementsCount}", GUI.skin.box );

        if( minE <= maxE )
        {
            var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
            rect.RequestTop( lh + 3 );
            GUILayout.Space( lh + 3 );
            GUI.Label( rect.VerticalSlice( 0, realSlices ), "Average", GUI.skin.box );
            var sliceId = 1;
            for( int j = minE; j <= maxE; j++ ) if( aggregated.HasChanceOfAnyElementCount(j) ) GUI.Label( rect.VerticalSlice( sliceId++, realSlices ), $"{j}", GUI.skin.box );
            EditorGUILayout.EndHorizontal();

            for( int i = 0; i < count; i++ )
            {
                rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
                rect.RequestTop( lh );
                GUILayout.Space( lh );
                GUI.Label( rect.VerticalSlice( 0, realSlices ), $"{aggregated._elementAvg[i]:N2} {aggregated._namesCache[i]}" );
                sliceId = 1;
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

public class SubsetSelectorPropAdapter : ISubsetSelector<ScriptableObject>
{
	SerializedProperty _prop;
	SerializedProperty _rollsProp;
	SerializedProperty _minProp;
	SerializedProperty _maxProp;
	SerializedProperty _weightsProp;
	SerializedProperty _rangeProp;
	SerializedProperty _entriesProp;
	List<EntryAdapter> _adapters = new();

    public int Min => _minProp.intValue;
    public int Max => _maxProp.intValue;
    public bool IsBiased => _weightsProp.arraySize > 0;
    public int EntriesCount => _entriesProp.arraySize;

    public SerializedProperty Prop => _prop;
	
    public void SetProp(SerializedProperty serializedProperty)
    {
        _prop = serializedProperty;
		_entriesProp = _prop.FindPropertyRelative("_entries");
		_rollsProp = _prop.FindPropertyRelative("_rolls");
		_weightsProp = _rollsProp.FindPropertyRelative("_weights");
		_rangeProp = _rollsProp.FindPropertyRelative("range");
		_minProp = _rangeProp.FindPropertyRelative("min");
		_maxProp = _rangeProp.FindPropertyRelative("max");
    }

    public float GetBiasWeight(int rolls) => _weightsProp.GetArrayElementAtIndex(rolls - Min).floatValue;

    public IWeightedSubsetEntry<ScriptableObject> GetEntry(int id)
    {
		EnsureSize();
		var adapter = _adapters[id];
		var prop = _entriesProp.GetArrayElementAtIndex( id );
		if( adapter.Prop != prop ) adapter.SetProp( prop );
		return adapter;
    }

    private void EnsureSize()
    {
		var count = EntriesCount;
		while( _adapters.Count < count )
		{
			var adapter = new EntryAdapter();
			adapter.SetProp( _entriesProp.GetArrayElementAtIndex( _adapters.Count ) );
			_adapters.Add( adapter );
		}
    }

    public IWeightedSubsetEntry GetEntryObject(int id) => GetEntry(id);

    class EntryAdapter : IWeightedSubsetEntry<ScriptableObject>
    {
		SerializedProperty _entryProp;
		SerializedProperty _weightProp;
		SerializedProperty _guaranteedProp;
		SerializedProperty _capProp;
		SerializedProperty _elementProp;

        public ScriptableObject Element => _elementProp.objectReferenceValue as ScriptableObject;
        public object ElementAsObject => _elementProp.objectReferenceValue;
        public int Guaranteed => _guaranteedProp.intValue;
        public int Cap => _capProp.intValue;
        public bool IsValid => _elementProp.objectReferenceValue != null;
        public float Weight => _weightProp.floatValue;

        public SerializedProperty Prop => _entryProp;

        public void SetProp(SerializedProperty serializedProperty)
        {
        	_entryProp = serializedProperty;
			_weightProp = _entryProp.FindPropertyRelative( "_weight" );
			_guaranteedProp = _entryProp.FindPropertyRelative("_guaranteed");
			_capProp = _entryProp.FindPropertyRelative("_cap");
			_elementProp = _entryProp.FindPropertyRelative("_element");
        }
    }
}

public class AggregatedSubsetSelectorPropAdapter : IAggregatedSubsetSelector<ScriptableObject>
{
	SerializedProperty _prop;
	List<SubsetSelectorPropAdapter> _adapters = new();

    public SerializedProperty Prop => _prop;
    public int Count => _prop.arraySize;

    public Type ElementType => typeof(ScriptableObject);

    public ISubsetSelector<ScriptableObject> GetEntry(int id)
    {
		EnsureSize();
		var adapter = _adapters[id];
		var prop = _prop.GetArrayElementAtIndex( id );
		if( adapter.Prop != prop ) adapter.SetProp( prop );
		return adapter;
    }

    private void EnsureSize()
    {
		var count = Count;
		while( _adapters.Count < count )
		{
			var adapter = new SubsetSelectorPropAdapter();
			adapter.SetProp( _prop.GetArrayElementAtIndex( _adapters.Count ) );
			_adapters.Add( adapter );
		}
    }

    public ISubsetSelector GetEntryObject(int i) => GetEntry(i);

	public void SetProp( SerializedProperty prop )
	{
		_prop = prop;
	}
}

public class PredictionsAggregator
{
	public AggregatedPredictor<ScriptableObject> aggregatedPredictor = new();
	AggregatedSubsetSelectorPropAdapter _adaptor = new();

	public int Count => _adaptor.Count;
    public SerializedProperty Prop => _adaptor.Prop;

	public SubsetSelectorPredictor<ScriptableObject> GetEntry( int index ) => aggregatedPredictor.GetSubPredictor( index );

	public void SetProp( SerializedProperty prop )
	{
		_adaptor.SetProp( prop );
		Debug.Log( $"SetProp( {prop} )" );
	}

	public void SetDirty()
	{
		
	}

    public void Calculate()
    {
		Debug.Log( $"Calculate( {aggregatedPredictor} )" );
		aggregatedPredictor.Calculate( _adaptor );
    }
}
