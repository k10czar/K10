using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceBehavior : MonoBehaviour
{
    void OnEnable()
	{
		ServiceLocator.Register( this );
	}

	void OnDisable()
	{
		ServiceLocator.Unregister( this );
	}
}

public class ServicesProvider : KomposedDebugableMonoBehavior, IDrawGizmosOnSelected, IDrawGizmos, ILogglable<ServicesLogCategory>
{
	[ExtendedDrawer, SerializeReference] IService[] _services;

    void Awake()
	{
		RegisterServices();
	}

	void Start()
	{
		if (_services == null) return;
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if ( service is IStartable startable )
			{
				if( service is IActivatable act && !act.IsActive.Value ) continue;
				try
				{
					startable.Start();
				}
				catch( Exception e ) 
				{
					this.LogException( e );
				}
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
			// if( service is IActivatable act )
			// {
			// 	if( !act.IsActive.Value ) continue;
			// 	if( service is IStartableService startable && !startable.IsReady ) startable.Start();
			// }
			if (service is IUpdatable updatable )
			{
				updatable.Update( deltaTime );
			}
		}
	}

	void RegisterServices()
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