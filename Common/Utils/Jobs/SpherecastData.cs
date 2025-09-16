using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SpherecastData
{
	public NativeArray<RaycastHit> results;
	public NativeArray<SpherecastCommand> commands;
	public JobHandle jobHandle;

	private int _size = 0;
	private bool _isCreated;

	public int Size { [MethodImpl(Optimizations.INLINE_IF_CAN)] get { return _size; } }
	public bool IsCreated { [MethodImpl(Optimizations.INLINE_IF_CAN)] get { return _isCreated; } }

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Prepare(int requestedSize)
	{
		if (requestedSize == _size) return;
		if (results.IsCreated) results.Dispose();
		if (commands.IsCreated) commands.Dispose();
		_size = requestedSize;
		results = new(_size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		commands = new(_size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_isCreated = true;
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public void Dispose()
	{
		if (!_isCreated) return;
		_isCreated = false;
		if (results.IsCreated) results.Dispose();
		if (commands.IsCreated) commands.Dispose();
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public JobHandle Schedule(int minCommandsPerJob, JobHandle dependsOn = default)
	{
		jobHandle = SpherecastCommand.ScheduleBatch(commands, results, minCommandsPerJob, dependsOn);
		return jobHandle;
	}
}