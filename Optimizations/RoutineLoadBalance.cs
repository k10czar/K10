// #if CODE_METRICS
// #define LOG
// #endif
using System.Collections.Generic;
using UnityEngine;

public class RoutineLoadBalance : System.IDisposable
{
	float msBudgetPerFrame;
	System.Diagnostics.Stopwatch _stopwatch;

	int _runningFrame = -1;
	double totalRunMs = 0;
	int rests = 0;
// #if LOG
// 	Stack<string> _stack = new Stack<string>();
// #endif

	public RoutineLoadBalance(float milisecondsBudgetPerCycle = 5)
	{
		msBudgetPerFrame = milisecondsBudgetPerCycle;
	}

	public bool CanRun => _stopwatch == null || _stopwatch.Elapsed.TotalMilliseconds < msBudgetPerFrame;

	public System.Collections.IEnumerator Rest()
	{
		totalRunMs += _stopwatch?.Elapsed.TotalMilliseconds ?? 0;
		yield return null;
		rests++;
		_stopwatch?.Restart();
	}

	public void StartBlock(string blockName)
	{
		if (_stopwatch == null)
		{
			_stopwatch = StopwatchPool.RequestStarted();
			return;
		}
		var frame = Time.frameCount;
		if (frame != _runningFrame)
		{
			_runningFrame = frame;
			_stopwatch.Restart();
		}
		else _stopwatch.Start();
// #if LOG
// 		_stack.Push(blockName);
// #endif
	}

	public void EndBlock()
	{
// #if LOG
// 		_stack.Pop();
// #endif
		if (_stopwatch == null) return;
		_stopwatch.Stop();
	}

	public void Dispose()
	{
		_stopwatch.ReturnToPool();
	}
}