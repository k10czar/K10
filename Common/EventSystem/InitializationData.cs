using System;
using System.Collections.Generic;

public struct InitializationData
{
	public bool _isReady;
	List<Action> _onReady;

	public void MakeReady()
	{
		_isReady = true;
		if (_onReady == null) return;
		foreach (var act in _onReady) act();
		_onReady.Clear();
		_onReady = null;
	}

	public void Kill()
	{
		_isReady = true;
		if (_onReady == null) return;
		_onReady.Clear();
		_onReady = null;
	}

	public void CallOnceReady(Action act)
	{
		if (_isReady)
		{
			act();
			return;
		}
		_onReady ??= new();
		_onReady.Add(act);
	}
}
