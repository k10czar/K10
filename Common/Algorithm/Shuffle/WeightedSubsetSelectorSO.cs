using K10;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public interface ISubsetSelector
{
    public int Min { get; }
    public int Max { get; }
    public float GetBiasWeight(int rolls);
    public bool IsBiased { get; }
    public int EntriesCount { get; }
    public IWeightedSubsetEntry GetEntryObject(int id);
    // public IntRng Rng { get; }
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
    public static bool IsEmpty(this ISubsetSelector selector) => selector.EntriesCount == 0;

    public static bool IsEverMaxRoll(this ISubsetSelector selector)
    {
        var maxRoll = 0;
        for (int i = 0; i < selector.EntriesCount; i++)
        {
            var entry = selector.GetEntryObject(i);
            maxRoll += entry.Cap;
        }
        return maxRoll <= selector.Min;
    }
    
    public static string Stringfy(this ISubsetSelector selector)
    {
        if (selector.IsEverMaxRoll())
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

        if( selector.IsBiased )
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
        else
        {
            if( selector.Min == selector.Max ) return $"{selector.Max} of {{ {elements} }}";
            else return $"[{selector.Min},{selector.Max}] of {{ {elements} }}";
        }
        return $"UNDENTIFIED";
    }

    public static IEnumerable<T> Roll<T>(this ISubsetSelector<T> selector) => ((ISubsetSelector)selector).Roll<T>();
    public static IEnumerable<T> Roll<T>(this ISubsetSelector selector)
    {
        var entriesCount = selector.EntriesCount;

        var rolls = 0;

        if( selector.IsBiased )
        {
            var sumWeights = 0f;
            for (int i = selector.Min; i <= selector.Max; i++ ) sumWeights += selector.GetBiasWeight(i);
            var rng = K10Random.Value * sumWeights;
            for (rolls = selector.Min; rolls < selector.Max; rolls++)
            {
                rng -= selector.GetBiasWeight(rolls);
                if (rng < 0 || MathAdapter.Approximately(rng, 0)) break;
            }
        }
        else
        {
            rolls = K10Random.Interval(selector.Min, selector.Max + 1);
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
    [SerializeField] IntRng _rolls;

    public int Min => _rolls.range.min;
    public int Max => _rolls.range.max;
    public bool IsBiased => _rolls.IsBiased;

    public float GetBiasWeight(int rolls) => _rolls.GetBiasWeight(rolls);
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
    [SerializeField] WeightedSubsetEntry<T>[] _entries;
    [SerializeField] IntRng _rolls;

    public int Min => _rolls.range.min;
    public int Max => _rolls.range.max;
    public int EntriesCount => _entries.Length;
    public bool IsBiased => _rolls.IsBiased;
    public IWeightedSubsetEntry<T> GetEntry(int id) => _entries[id];
    public IWeightedSubsetEntry GetEntryObject(int id) => GetEntry(id);

    public float GetBiasWeight(int rolls) => _rolls.GetBiasWeight( rolls );
    public override string ToString() => this.Stringfy();
}
