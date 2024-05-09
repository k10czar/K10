using System.Collections.Generic;
using UnityEngine;

public class ServicesProvider : KomposedDebugableMonoBehavior, IDrawGizmosOnSelected, IDrawGizmos
{
	[ExtendedDrawer, SerializeReference] IService[] _services;

    void Awake()
	{
		StartServices();
	}

	void Start()
	{
		if (_services == null) return;
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if (service is IStartable startable ) 
			{
				if( service is IActivatable act && !act.IsActive.Value ) continue;
				startable.Start();
			}
		}
	}

	void Update()
	{
		if (_services == null) return;
		var deltaTime = Time.unscaledDeltaTime;
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if (service is IUpdatable updatable ) 
			{
				if( service is IActivatable act )
				{
					if( !act.IsActive.Value ) continue;
					else if( service is IStartable startable && !startable.IsStarted ) startable.Start();
				}
				updatable.Update( deltaTime );
			}
		}
	}

	void StartServices()
	{
		if (_services == null) return;
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if (service == null) continue;
			ServiceLocator.Register(service);
		}
	}

	void OnDestroy()
	{
		KillServices();
	}
    
    protected override IEnumerable<object> GetKomposedDebugableObjects()
    {
        for( int i = 0; i < _services.Length; i++ ) yield return _services[i];
    }

	private void KillServices()
	{
		if (_services == null) return;
		for (int i = 0; i < _services.Length; i++)
		{
			var service = _services[i];
			if (service == null) continue;
			ServiceLocator.Unregister(service);
			if (service is ICustomDisposableKill killable) killable.Kill();
			_services[i] = null;
		}
		_services = null;
	}
}
