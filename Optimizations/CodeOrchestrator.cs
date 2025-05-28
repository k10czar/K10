using System;
using System.Collections.Generic;
using UnityEngine;

public interface IOrchestratedUpdate
{
	void PreUpdate();
	void PostUpdate();
}

public interface IOrchestratedLateUpdate
{
	void PreLateUpdate();
	void PostLateUpdate();
}

public interface IOrchestratedFixedUpdate
{
	void PreFixedUpdate();
	void PostFixedUpdate();
}

public class CodeOrchestrator : MonoBehaviour
{
	List<IOrchestratedUpdate> _updatables = new();
	List<IOrchestratedUpdate> _updatablesCallList = new();
	List<IOrchestratedLateUpdate> _lateUpdatables = new();
	List<IOrchestratedLateUpdate> _lateUpdatablesCallList = new();
	List<IOrchestratedFixedUpdate> _fixedUpdatables = new();
	List<IOrchestratedFixedUpdate> _fixedUpdatablesCallList = new();

	// EventSlot _onDestroy;
	// public IEventRegister OnDestroy => _onDestroy ??= new();

	static CodeOrchestrator _sceneOrchestrator;
	static CodeOrchestrator _eternalOrchestrator;

	public static CodeOrchestrator Scene
	{
		get
		{
			if( _sceneOrchestrator == null )
			{
				GameObject obj = new GameObject( $"SceneCodeOrchestrator" );
				_sceneOrchestrator = obj.AddComponent<CodeOrchestrator>();
				// var buildOrchestrator = _sceneOrchestrator;
				// _sceneOrchestrator.OnDestroy.Register( () => {
				// 	if( buildOrchestrator == _sceneOrchestrator ) _sceneOrchestrator = null;
				// } );
			}
			return _sceneOrchestrator;
		}
	}

	public static CodeOrchestrator Eternal
	{
		get
		{
			if (_eternalOrchestrator == null)
			{
				GameObject obj = new GameObject($"EternalCodeOrchestration");
				_eternalOrchestrator = obj.AddComponent<CodeOrchestrator>();
				DontDestroyOnLoad( obj );
			}
			return _eternalOrchestrator;
		}
	}

	public void Add( object obj )
	{
		if( obj is IOrchestratedUpdate upd ) _updatables.Add( upd );
		if( obj is IOrchestratedLateUpdate lupd ) _lateUpdatables.Add( lupd );
		if( obj is IOrchestratedFixedUpdate fupd ) _fixedUpdatables.Add( fupd );
	}

	public void Remove( object obj )
	{
		if( obj is IOrchestratedUpdate upd ) _updatables.Remove( upd );
		if( obj is IOrchestratedLateUpdate lupd ) _lateUpdatables.Remove( lupd );
		if( obj is IOrchestratedFixedUpdate fupd ) _fixedUpdatables.Remove( fupd );
	}

    void OnDestroy()
    {
		for (int i = 0; i < _updatables.Count; i++) if (_updatables[i] is IDisposable disp) disp.Dispose();
		for (int i = 0; i < _lateUpdatables.Count; i++) if (_lateUpdatables[i] is IDisposable disp) disp.Dispose();
		for (int i = 0; i < _fixedUpdatables.Count; i++) if (_fixedUpdatables[i] is IDisposable disp) disp.Dispose();
    }

    void Update()
	{
		var len = _updatables.Count;
		
		while (_updatablesCallList.Count < len) _updatablesCallList.Add(null);
		for (int i = 0; i < len; i++) _updatablesCallList[i] = _updatables[i];

		for (int i = 0; i < len; i++)
		{
			var element = _updatablesCallList[i];
			element.PreUpdate();
		}
		for (int i = 0; i < len; i++)
		{
			var element = _updatablesCallList[i];
			element.PostUpdate();
			_updatablesCallList[i] = null;
		}
	}

	void FixedUpdate()
	{
		var len = _lateUpdatables.Count;
		
		while (_lateUpdatablesCallList.Count < len) _lateUpdatablesCallList.Add(null);
		for (int i = 0; i < len; i++) _lateUpdatablesCallList[i] = _lateUpdatables[i];
		
		for (int i = 0; i < len; i++)
		{
			var element = _lateUpdatablesCallList[i];
			element.PreLateUpdate();
		}
		for( int i = 0; i < len; i++ )
		{
			var element = _lateUpdatablesCallList[i];
			element.PostLateUpdate();
			_lateUpdatablesCallList[i] = null;
		}
	}

	void LateUpdate()
	{
		var len = _fixedUpdatables.Count;
		
		while (_fixedUpdatablesCallList.Count < len) _fixedUpdatablesCallList.Add(null);
		for (int i = 0; i < len; i++) _fixedUpdatablesCallList[i] = _fixedUpdatables[i];
		
		for (int i = 0; i < len; i++)
		{
			var element = _fixedUpdatablesCallList[i];
			element.PreFixedUpdate();
		}
		for( int i = 0; i < len; i++ )
		{
			var element = _fixedUpdatablesCallList[i];
			element.PostFixedUpdate();
			_fixedUpdatablesCallList[i] = null;
		}
	}
}
