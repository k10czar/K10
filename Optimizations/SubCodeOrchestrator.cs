// #define DEBUG
using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

public class SubCodeOrchestrator : IOrchestratedUpdate, IOrchestratedLateUpdate, IOrchestratedFixedUpdate, IDisposable
{
	BakedList<IOrchestratedUpdate> _updatables = new();
	BakedList<IOrchestratedLateUpdate> _lateUpdatables = new();
	BakedList<IOrchestratedFixedUpdate> _fixedUpdatables = new();

	[SerializeField, BlockEdit] int uLen;
	[SerializeField, BlockEdit] int luLen;
	[SerializeField, BlockEdit] int fuLen;

	public void Dispose()
	{
		_updatables.Dispose();
		_lateUpdatables.Dispose();
		_fixedUpdatables.Dispose();
	}

	public void DebugList(List<string> debugList)
	{
		InnerDebugUpdate(debugList, "-");
		InnerDebugLatUpdate(debugList, "-");
		InnerDebugFixedUpdate(debugList, "-");
	}

	void InnerDebugUpdate(List<string> debugList, string tabulation)
	{
		foreach (var element in _updatables.bakedList)
		{
			debugList.Add($"{tabulation}{element.ToStringOrNull()}.Update()");
			if (element is SubCodeOrchestrator so) so.InnerDebugUpdate(debugList, "  " + tabulation);
		}
	}

	void InnerDebugLatUpdate(List<string> debugList, string tabulation)
	{
		foreach (var element in _lateUpdatables.bakedList)
		{
			debugList.Add($"{tabulation}{element.ToStringOrNull()}.LateUpdate()");
			if (element is SubCodeOrchestrator so) so.InnerDebugUpdate(debugList, "  " + tabulation);
		}
	}

	void InnerDebugFixedUpdate(List<string> debugList, string tabulation)
	{
		foreach (var element in _fixedUpdatables.bakedList)
		{
			debugList.Add($"{tabulation}{element.ToStringOrNull()}.FixedUpdate()");
			if (element is SubCodeOrchestrator so) so.InnerDebugUpdate(debugList, "  " + tabulation);
		}
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Add(object obj)
	{
// #if DEBUG
// 		Debug.Log($"SubCodeOrchestrator.Add( {obj.ToStringOrNullColored(Colors.DodgerBlue)} ) u:{obj is IOrchestratedUpdate} lu:{obj is IOrchestratedLateUpdate} fu:{obj is IOrchestratedFixedUpdate}");
// #endif
		if (obj is IOrchestratedUpdate upd) _updatables.Add(upd);
		if (obj is IOrchestratedLateUpdate lupd) _lateUpdatables.Add(lupd);
		if (obj is IOrchestratedFixedUpdate fupd) _fixedUpdatables.Add(fupd);
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Remove(object obj)
	{
// #if DEBUG
// 		Debug.Log($"SubCodeOrchestrator.Remove( {obj.ToStringOrNullColored(Colors.Peru)} )");
// #endif
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
		_updatables.Bake();
		var list = _updatables.bakedList;
		uLen = list.Count;
		for (int i = 0; i < uLen; i++) list[i].PreUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostUpdate()
	{
		var list = _updatables.bakedList;
		uLen = list.Count;
		for (int i = 0; i < uLen; i++) list[i].PostUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PreLateUpdate()
	{
		_lateUpdatables.Bake();
		var list = _lateUpdatables.bakedList;
		luLen = list.Count;
		for (int i = 0; i < luLen; i++) list[i].PreLateUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostLateUpdate()
	{
		var list = _lateUpdatables.bakedList;
		luLen = list.Count;
		for (int i = 0; i < luLen; i++) list[i].PostLateUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PreFixedUpdate()
	{
		_fixedUpdatables.Bake();
		var list = _fixedUpdatables.bakedList;
		fuLen = list.Count;
		for (int i = 0; i < fuLen; i++) list[i].PreFixedUpdate();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void PostFixedUpdate()
	{
		var list = _fixedUpdatables.bakedList;
		fuLen = list.Count;
		for (int i = 0; i < fuLen; i++) list[i].PostFixedUpdate();
	}
}
