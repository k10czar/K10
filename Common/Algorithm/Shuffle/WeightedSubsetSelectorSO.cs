using K10;
using UnityEngine;
using System.Collections.Generic;
using System;

public enum ESubsetGeneratorRule
{
    FIXED_COUNT,
    SIMPLE_RANGE,
    BIASED_RANGE,
}

public interface ISubsetSelector
{
    public ESubsetGeneratorRule Rule { get; }
    public int Min { get; }
    public int Max { get; }
    public float GetBiasWeight(int rolls);
    public int EntriesCount { get; }
    public IWeightedSubsetEntry GetEntryObject(int id);
}

public interface ISubsetSelector<T> : ISubsetSelector
{
    public IWeightedSubsetEntry<T> GetEntry(int id);
}

public interface IWeightedSubsetEntry : IWeighted
{
    public object ElementAsObject { get; }
    public int Guaranteed { get; }
    public int Cap { get; }
    public bool IsValid { get; }
}

public interface IWeightedSubsetEntry<T> : IWeightedSubsetEntry
{
    public T Element { get; }
}

public static class SubsetSelectorExtension
{
    public static IEnumerable<T> Roll<T>(this ISubsetSelector<T> selector) => ((ISubsetSelector)selector).Roll<T>();
    public static IEnumerable<T> Roll<T>(this ISubsetSelector selector)
    {
        var entriesCount = selector.EntriesCount;
        if (selector.Rule == ESubsetGeneratorRule.FIXED_COUNT)
        {
            var fixedResult = new List<T>();
            for (int i = 0; i < entriesCount; i++)
            {
                var entry = selector.GetEntryObject(i);
                for (int j = 0; j < entry.Cap; j++) fixedResult.Add((T)entry.ElementAsObject);
            }
            return fixedResult;
        }

        var rolls = 0;
        switch (selector.Rule)
        {
            case ESubsetGeneratorRule.SIMPLE_RANGE:
                rolls = K10Random.Interval(selector.Min, selector.Max + 1);
                break;

            case ESubsetGeneratorRule.BIASED_RANGE:
                var sumWeights = 0f;
                for (int i = selector.Min; i <= selector.Max; i++ ) sumWeights += selector.GetBiasWeight(i);
                var rng = K10Random.Value * sumWeights;
                for (rolls = selector.Min; rolls < selector.Max; rolls++)
                {
                    rng -= selector.GetBiasWeight(rolls);
                    if (rng < 0 || MathAdapter.Approximately(rng, 0)) break;
                }
                break;
        }
        
        var variableResult = new List<T>(rolls);

        var times = ObjectPool<Dictionary<IWeightedSubsetEntry, int>>.Request();
        var elementsToRoll = ObjectPool<List<IWeightedSubsetEntry>>.Request();

        var guaranteedDrops = 0;
        var ignoredDrops = 0;
        for (int i = 0; i < entriesCount; i++)
        {
            var entry = selector.GetEntryObject(i);
            var guaranteedRolls = entry.Guaranteed;
            times[entry] = guaranteedRolls;
            if (entry.Cap < 0 || guaranteedRolls < entry.Cap) elementsToRoll.Add(entry);
            guaranteedDrops += guaranteedRolls;

            if (!entry.IsValid)
            {
                ignoredDrops += guaranteedRolls;
                continue;
            }

            for (int j = 0; j < guaranteedRolls; j++) variableResult.Add((T)entry.ElementAsObject);
        }

        var drops = guaranteedDrops;
        for (; elementsToRoll.Count > 0 && drops < rolls; drops++)
        {
            var id = elementsToRoll.RandomID();
            var entry = elementsToRoll[id];
            var entryRolls = times[entry] + 1;
            times[entry] = entryRolls;

            bool hitTheCap = (entry.Cap >= 0) && (entryRolls >= entry.Cap);
            if (hitTheCap) elementsToRoll.RemoveAt(id);

            if (!entry.IsValid)
            {
                ignoredDrops++;
                continue;
            }

            variableResult.Add((T)entry.ElementAsObject);
        }

        ObjectPool.Return(elementsToRoll);
        ObjectPool.Return(times);

        return variableResult;
    }
}

public class BaseWeightedSubsetSelectorSO : ScriptableObject { }

public class WeightedSubsetSelectorSO<T> : BaseWeightedSubsetSelectorSO, ISubsetSelector<T> where T : ScriptableObject
{
    [SerializeField] ESubsetGeneratorRule _rule;
    [SerializeField] int _min;
    [SerializeField] int _max;
    [SerializeField] Entry[] _entries;
    [SerializeField] float[] _rangeWeights;

    public ESubsetGeneratorRule Rule => _rule;
    public int Min => _min;
    public int Max => _max;
    public int EntriesCount => _entries.Length;
    public IWeightedSubsetEntry<T> GetEntry(int id) => _entries[id];
    public IWeightedSubsetEntry GetEntryObject(int id) => GetEntry(id);

    public float GetBiasWeight(int rolls)
    {
        var id = rolls - _min;
        if (id < 0) return 0;
        var len = _rangeWeights.Length;
        if (len == 0) return 1;
        if( id >= len ) return _rangeWeights[len-1];
        return _rangeWeights[id];
    }

    [System.Serializable]
    public class Entry : IWeightedSubsetEntry<T>
    {
        [SerializeField] T _element;
        [SerializeField] int _guaranteed = 0;
        [SerializeField] int _cap = -1;
        [SerializeField] float _weight = 1;

        public bool IsValid => _element != null;
        public T Element => _element;
        public object ElementAsObject => _element;
        public int Guaranteed => _guaranteed;
        public int Cap => _cap;
        public float Weight => _weight;
    }
}
