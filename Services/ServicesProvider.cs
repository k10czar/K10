using System;
using System.Collections.Generic;
using K10.DebugSystem;
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

public class ServicesProvider : KomposedDebugableMonoBehavior, IDrawGizmosOnSelected, IDrawGizmos, ILoggable<ServicesLogCategory>
{
	static List<ServicesProvider> _providers = new();

	[ExtendedDrawer, SerializeReference] IService[] _services;

    protected override bool CanDrawGizmos => this.CanLogVisuals();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
	{
		_providers.Clear();
	}

    void Awake()
	{
		RegisterServices();
	}

	void Start()
	{
		if (_services == null) return;
		_providers.Add( this );
		this.Log( $"{this.HierarchyNameOrNullColored(Colors.Console.Fields)} starting services" );
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			this.Log( $"Starting service <{service.TypeNameOrNullColored(Colors.Console.TypeName)}>: {service.ToStringOrNull()}" );
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
			if( service is IActivatable act && !act.IsActive.Value ) continue;
			if (service is IUpdatable updatable )
			{
				updatable.Update( deltaTime );
			}
		}
	}

	void LateUpdate()
	{
		if (_services == null) return;
		var deltaTime = Time.unscaledDeltaTime;
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if( service is IActivatable act && !act.IsActive.Value ) continue;
			if (service is ILateUpdatable updatable )
			{
				updatable.LateUpdate( deltaTime );
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
		_providers.Remove( this );
	}

	public static void OnPreRender( Camera cam )
	{
		foreach( var prov in _providers ) 
			prov.ExecuteOnPreRender( cam );
	}

	void ExecuteOnPreRender( Camera cam )
	{
		for( int i = 0; i < _services.Length; i++ )
		{
			var service = _services[i];
			if( service is IActivatable act && !act.IsActive.Value ) continue;
			if (service is IOnPreRender renderable ) renderable.OnPreRender( cam );
		}
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
			if (service is IDisposable disposable) disposable.Dispose();
			_services[i] = null;
		}
		_services = null;
	}
}