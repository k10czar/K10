using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class BakedList<T> : IDisposable
{
	List<T> originals = new();
	public readonly List<T> bakedList = new();
	bool _dirty = false;

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Bake()
	{
		if (!_dirty) return;
		var len = originals.Count;
		while (bakedList.Count < len) bakedList.Add(default);
		for (int i = 0; i < len; i++) bakedList[i] = originals[i];
		_dirty = false;
	}

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Add(T t)
	{
		originals.Add(t);
		_dirty = true;
	}

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
	public bool Remove(T t)
	{
		var removed = originals.Remove(t);
		_dirty |= removed;
		return removed;
	}

    [MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Dispose()
	{
		for (int i = 0; i < originals.Count; i++) if (originals[i] is IDisposable disp) disp.Dispose();
		originals.Clear();
		bakedList.Clear();
		_dirty = false;
	}
}
