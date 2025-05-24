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
	List<IOrchestratedLateUpdate> _lateUpdatables = new();
	List<IOrchestratedFixedUpdate> _fixedUpdatables = new();

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

	void Update()
	{
		var len = _updatables.Count;
		for( int i = 0; i < len; i++ )
		{
			var element = _updatables[i];
			element.PreUpdate();
		}
		for( int i = 0; i < len; i++ )
		{
			var element = _updatables[i];
			element.PostUpdate();
		}
	}

	void FixedUpdate()
	{
		var len = _lateUpdatables.Count;
		for( int i = 0; i < len; i++ )
		{
			var element = _lateUpdatables[i];
			element.PreLateUpdate();
		}
		for( int i = 0; i < len; i++ )
		{
			var element = _lateUpdatables[i];
			element.PostLateUpdate();
		}
	}

	void LateUpdate()
	{
		var len = _fixedUpdatables.Count;
		for( int i = 0; i < len; i++ )
		{
			var element = _fixedUpdatables[i];
			element.PreFixedUpdate();
		}
		for( int i = 0; i < len; i++ )
		{
			var element = _fixedUpdatables[i];
			element.PostFixedUpdate();
		}
	}
}
