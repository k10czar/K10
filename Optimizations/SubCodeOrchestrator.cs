using System;
using System.Runtime.CompilerServices;

public class SubCodeOrchestrator : IOrchestratedUpdate, IOrchestratedLateUpdate, IOrchestratedFixedUpdate, IDisposable
{
	BakedList<IOrchestratedUpdate> _updatables = new();
	BakedList<IOrchestratedLateUpdate> _lateUpdatables = new();
	BakedList<IOrchestratedFixedUpdate> _fixedUpdatables = new();

	public void Dispose()
	{
		_updatables.Dispose();
		_lateUpdatables.Dispose();
		_fixedUpdatables.Dispose();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Add(object obj)
	{
		if (obj is IOrchestratedUpdate upd) _updatables.Add(upd);
		if (obj is IOrchestratedLateUpdate lupd) _lateUpdatables.Add(lupd);
		if (obj is IOrchestratedFixedUpdate fupd) _fixedUpdatables.Add(fupd);
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Remove(object obj)
	{
		if (obj is IOrchestratedUpdate upd) _updatables.Remove(upd);
		if (obj is IOrchestratedLateUpdate lupd) _lateUpdatables.Remove(lupd);
		if (obj is IOrchestratedFixedUpdate fupd) _fixedUpdatables.Remove(fupd);
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)] public void BakeUpdateList() { _updatables.Bake(); }
	[MethodImpl(Optimizations.INLINE_IF_CAN)] public void BakeLateUpdateList() { _lateUpdatables.Bake(); }
	[MethodImpl(Optimizations.INLINE_IF_CAN)] public void BakeFixedUpdateList() { _fixedUpdatables.Bake(); }


	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PreUpdate()
	{
		var list = _updatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PreUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostUpdate()
	{
		var list = _updatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PostUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PreLateUpdate()
	{
		var list = _lateUpdatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PreLateUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostLateUpdate()
	{
		var list = _lateUpdatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PostLateUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PreFixedUpdate()
	{
		var list = _fixedUpdatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PreFixedUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostFixedUpdate()
	{
		var list = _fixedUpdatables.bakedList;
		var len = list.Count;
		for (int i = 0; i < len; i++) list[i].PostFixedUpdate();
	}
}
