using K10;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public enum ESubsetGeneratorRule
{
    MAX_ROLL,
    FIXED_ROLLS,
    UNIFORM_RANGE,
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
    public static bool IsEmpty(this ISubsetSelector selector) => selector.EntriesCount == 0 || (selector.Rule != ESubsetGeneratorRule.FIXED_ROLLS && selector.Max == 0);
    
    public static string Stringfy(this ISubsetSelector selector)
    {
        if (selector.Rule == ESubsetGeneratorRule.MAX_ROLL)
        {
            var sbmr = StringBuilderPool.RequestEmpty();
            sbmr.Append("{");
            for (int i = 0; i < selector.EntriesCount; i++)
            {
                var entry = selector.GetEntryObject(i);
                sbmr.Append($"{(i > 0 ? ", " : "")}{entry.Cap}:{entry.ElementAsObject.ToStringOrNull()}");
            }
            sbmr.Append("}");
            return sbmr.ReturnToPoolAndCast();
        }
        var totalWeight = 0f;
        for (int i = 0; i < selector.EntriesCount; i++) totalWeight += selector.GetEntryObject(i).Weight;
        
        var sb = StringBuilderPool.RequestEmpty();
        for (int i = 0; i < selector.EntriesCount; i++)
        {
            var entry = selector.GetEntryObject(i);
            sb.Append($"{(i > 0 ? ", " : "")}{entry.ElementAsObject.ToStringOrNull()}[{entry.Guaranteed},{entry.Cap}]({entry.Weight*100/totalWeight:N0}%)");
        }
        var elements = sb.ReturnToPoolAndCast();
        switch (selector.Rule)
        {
            case ESubsetGeneratorRule.FIXED_ROLLS: return $"{selector.Max} of {{ {elements} }}";
            case ESubsetGeneratorRule.UNIFORM_RANGE: return $"[{selector.Min},{selector.Max}] of {{ {elements} }}";
        }
        if (selector.Rule == ESubsetGeneratorRule.BIASED_RANGE)
        {
            var ranges = $"[{selector.Min},{selector.Max}]";
            var sumRangesWeights = 0f;
            var delta = selector.Max + 1 - selector.Min;
            for (int i = 0; i < delta; i++) sumRangesWeights += selector.GetBiasWeight( i );
            if ( !MathAdapter.Approximately( sumRangesWeights, 0 ) )
            {
                sb = StringBuilderPool.RequestEmpty();
                sb.Append("{");
                for (int i = 0; i < delta; i++) sb.Append($"{(i > 0 ? ", " : "")}{i + selector.Min}({100 * selector.GetBiasWeight(i) / sumRangesWeights:N0}%)");
                sb.Append("}");
                ranges = sb.ReturnToPoolAndCast();
            }
            return $"{ranges} of {{ {elements} }}";
        }
        return $"UNDENTIFIED";
    }

    public static IEnumerable<T> Roll<T>(this ISubsetSelector<T> selector) => ((ISubsetSelector)selector).Roll<T>();
    public static IEnumerable<T> Roll<T>(this ISubsetSelector selector)
    {
        var entriesCount = selector.EntriesCount;
        if (selector.Rule == ESubsetGeneratorRule.MAX_ROLL)
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
            case ESubsetGeneratorRule.FIXED_ROLLS:
                rolls = selector.Max;
                break;

            case ESubsetGeneratorRule.UNIFORM_RANGE:
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

[Serializable]
public class WeightedSubsetEntry<T> : IWeightedSubsetEntry<T>
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

public abstract class BaseWeightedSubsetSelectorSO : ScriptableObject
{
    [SerializeField] ESubsetGeneratorRule _rule;
    [SerializeField] int _min;
    [SerializeField] int _max;
    [SerializeField] float[] _rangeWeights;

    public ESubsetGeneratorRule Rule => _rule;
    public int Min => _min;
    public int Max => _max;

    public float GetBiasWeight(int rolls)
    {
        var id = rolls - _min;
        if (id < 0) return 0;
        var len = _rangeWeights.Length;
        if (len == 0) return 1;
        if( id >= len ) return _rangeWeights[len-1];
        return _rangeWeights[id];
    }
}

public class WeightedSubsetSelectorSO<T> : BaseWeightedSubsetSelectorSO, ISubsetSelector<T> where T : ScriptableObject
{
    [SerializeField] WeightedSubsetEntry<T>[] _entries;

    public int EntriesCount => _entries.Length;
    public IWeightedSubsetEntry<T> GetEntry(int id) => _entries[id];
    public IWeightedSubsetEntry GetEntryObject(int id) => GetEntry(id);
}

[Serializable]
public class WeightedSubsetSelector<T> : ISubsetSelector<T>
{
    [SerializeField] ESubsetGeneratorRule _rule;
    [SerializeField] int _min;
    [SerializeField] int _max;
    [SerializeField] WeightedSubsetEntry<T>[] _entries;
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
        if (id >= len) return _rangeWeights[len - 1];
        return _rangeWeights[id];
    }

    public override string ToString() => this.Stringfy();
}
