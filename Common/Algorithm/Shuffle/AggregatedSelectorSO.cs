using UnityEngine;
using System;
using System.Collections.Generic;


public interface IAggregatedSubsetSelector
{
    public int Count { get; }
    System.Type ElementType { get; }
    ISubsetSelector GetEntryObject(int i);
}

public static class AggregatedSubsetSelectorExtensions
{
    public static bool IsEmpty(this IAggregatedSubsetSelector data)
    {
        if( data == null ) return true;
        var count = data.Count;
        if( count == 0 ) return true;
        for( int i = 0; i < count; i++)
        {
            var set = data.GetEntryObject(i);
            if( !set.IsEmpty() ) return false;
        }
        return true; 
    }

    public static IEnumerable<T> Roll<T>( this IAggregatedSubsetSelector data )
    {
        var roll = new List<T>();
        var count = data.Count;
        for( int i = 0; i < count; i++ )
        {
            var subSet = data.GetEntryObject(i);
            var subRoll = subSet.Roll<T>();
            roll.AddRange( subRoll );
        }
        return roll;
    }

    public static IEnumerable<T> Roll<T>( this IAggregatedSubsetSelector<T> data ) 
    {
        var dataGeneric = data as IAggregatedSubsetSelector;
        return dataGeneric.Roll<T>();
    }
}

public interface IAggregatedSubsetSelector<T> : IAggregatedSubsetSelector
{
    public ISubsetSelector<T> GetEntry(int id);
}

public abstract class BaseAggregatedSelectorSO : ScriptableObject, IAggregatedSubsetSelector
{
    public abstract int Count { get; }
    public abstract Type ElementType { get; }
    public abstract ISubsetSelector GetEntryObject(int i);
}

[System.Serializable]
public class AggregatedSelector<T> : IAggregatedSubsetSelector<T> where T : ScriptableObject
{
    [SerializeField] WeightedSubsetSelector<T>[] _entries;

    public Type ElementType => typeof(T);

    public int Count => _entries.Length;
    public ISubsetSelector GetEntryObject(int id) => _entries[id];
    public ISubsetSelector<T> GetEntry(int id) => _entries[id];

    public override string ToString() => $"Aggregated of {_entries.ToStringOrNull()}{(this.IsEmpty()?" EMPTY":"")}";
}

public abstract class AggregatedSelectorSO<T> : BaseAggregatedSelectorSO, IAggregatedSubsetSelector<T> where T : ScriptableObject
{
    [SerializeField] WeightedSubsetSelector<T>[] _entries;

    public override Type ElementType => typeof(T);

    public override int Count => _entries.Length;
    public override ISubsetSelector GetEntryObject(int id) => _entries[id];
    public ISubsetSelector<T> GetEntry(int id) => _entries[id];
}