using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SubsetSelectorPropAdapter<T> : ISubsetSelector<T> where T : ScriptableObject
{
	SerializedProperty _prop;
	SerializedProperty _rollsProp;
	SerializedProperty _minProp;
	SerializedProperty _maxProp;
	SerializedProperty _weightsProp;
	SerializedProperty _rangeProp;
	SerializedProperty _entriesProp;
	List<EntryAdapter<T>> _adapters = new();

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

    public IWeightedSubsetEntry<T> GetEntry(int id)
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
			var adapter = new EntryAdapter<T>();
			adapter.SetProp( _entriesProp.GetArrayElementAtIndex( _adapters.Count ) );
			_adapters.Add( adapter );
		}
    }

    public IWeightedSubsetEntry GetEntryObject(int id) => GetEntry(id);

    class EntryAdapter<T> : IWeightedSubsetEntry<T> where T : ScriptableObject
    {
		SerializedProperty _entryProp;
		SerializedProperty _weightProp;
		SerializedProperty _guaranteedProp;
		SerializedProperty _capProp;
		SerializedProperty _elementProp;

        public T Element => (T)_elementProp.objectReferenceValue;
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

public class AggregatedSubsetSelectorPropAdapter<T> : IAggregatedSubsetSelector<T> where T : ScriptableObject
{
	SerializedProperty _prop;
	List<SubsetSelectorPropAdapter<T>> _adapters = new();

    public SerializedProperty Prop => _prop;
    public int Count => _prop.arraySize;

    public Type ElementType => typeof(ScriptableObject);

    public ISubsetSelector<T> GetEntry(int id)
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
			var adapter = new SubsetSelectorPropAdapter<T>();
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

public class PredictionsAggregator<T> where T : ScriptableObject
{
	public IAggregatedPredictor<T> aggregatedPredictor = new AggregatedPredictor<T>();
	AggregatedSubsetSelectorPropAdapter<T> _adaptor = new();

	public int Count => _adaptor.Count;
    public SerializedProperty Prop => _adaptor.Prop;

	public PredictionsAggregator( IAggregatedPredictor<T> predictor )
	{
		aggregatedPredictor = predictor ?? new AggregatedPredictor<T>();
	}

	public PredictionsAggregator() : this(new AggregatedPredictor<T>()) { }

	public void SetProp( SerializedProperty prop )
	{
		_adaptor.SetProp( prop );
	}

	public void SetDirty()
	{
		
	}

    public void Calculate()
    {
		aggregatedPredictor.Calculate( _adaptor );
    }
}
