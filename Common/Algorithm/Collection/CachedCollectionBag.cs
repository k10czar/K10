using System.Collections.Generic;
using UnityEngine;

public interface ICachedCollectionBagObserver<T>
{
	int Count { get; }
	int[] ShuffledOrder { get; }
	int Index { get; }
	IEventRegister<int[]> OnBagShuffled { get; }
	IEventRegister<T> OnElementAdded { get; }
	IEventRegister<T> OnElementPulled { get; }
}

public interface ICachedCollectionBag<T> : ICachedCollectionBagObserver<T>
{
	void Add( T t );
	T Pull();
	void ShuffleBag();
}

public class CachedCollectionBag<T> : ICachedCollectionBag<T>
{
    protected List<T> _data = new List<T>();

    protected EventSlot<int[]> _onBagShuffled = new EventSlot<int[]>();
    public IEventRegister<int[]> OnBagShuffled { get { return _onBagShuffled; } }

    protected EventSlot<T> _onElementAdded = new EventSlot<T>();
    protected EventSlot<T> _onElementPulled = new EventSlot<T>();

    public IEventRegister<T> OnElementAdded { get { return _onElementAdded; } }
    public IEventRegister<T> OnElementPulled { get { return _onElementPulled; } }

    protected List<int> _shuffledOrder = new List<int>();
    private int _timesShuffled = 0;
    public int[] ShuffledOrder { get { return _shuffledOrder.ToArray(); } }

    protected int _index = 0;
    public int Index { get { return _index; } }
    public int Count { get { return _data.Count; } }
    
    public void Add(T t)
    {
        _data.Add(t);
        _onElementAdded.Trigger(t);
    }
    
    public T Pull()
    {
        if(_index >= _shuffledOrder.Count || _timesShuffled == 0)
        {
            ShuffleBag();
        }

        var t = _data[_shuffledOrder[_index]];
        _index++;
        _onElementPulled.Trigger(t);
        return t;
    }

    public void ShuffleBag()
    {
        // Debug.Log("BAG OF SIZE " + Count + " BEING SHUFFLED!");
        _index = 0;
        _timesShuffled++;
        _shuffledOrder.Clear();

        for (int i = 0; i < _data.Count; i++) {
            _shuffledOrder.Add(i);
        }

        for(int i = 0; i < _shuffledOrder.Count; i++)
        {
            var temp = _shuffledOrder[i];
            var rnd = Random.Range(i, _shuffledOrder.Count);

            _shuffledOrder[i] = _shuffledOrder[rnd];
            _shuffledOrder[rnd] = temp;
        }

        _onBagShuffled.Trigger(_shuffledOrder.ToArray());
    }

    public CachedCollectionBag<T> BuildPremadeBag(List<T> data, int[] order, int ind)
    {
        CachedCollectionBag<T> bag = new CachedCollectionBag<T>();
        _data = data;
        _shuffledOrder = new List<int>(order);
        _index = ind;
        _timesShuffled = 1;
        return bag;
    }
}