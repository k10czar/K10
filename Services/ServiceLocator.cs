using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K10.DebugSystem;
using UnityEngine;
using static Colors.Console;

public class ServicesLogCategory : IK10LogCategory
{
    public string Name => "💂Services";
    public Color Color => Colors.Orange;
}

public static class ServiceLocator
{
	private static readonly Type ServType = typeof(IService);

	[ResetedOnLoad] private static readonly Dictionary<Type, IService> services = new();
	[ResetedOnLoad] private static readonly Dictionary<Type, Dictionary<Type[], IService>> genericServices = new();

	[ResetedOnLoad] private static readonly Dictionary<Type, List<Action>> onServiceRegisteredCallbacks = new();
	[ResetedOnLoad] private static readonly Dictionary<Type, List<Action>> onServiceReadyCallbacks = new();

	static EventSlot<float> _onUpdate = null;

	public static IEventRegister<float> OnUpdate
	{
		get
		{
			if( _onUpdate == null )
			{
				_onUpdate = new();
				var go = new GameObject( "ServiceUpdater" );
				UnityEngine.Object.DontDestroyOnLoad( go );
				var relay = go.AddComponent<UpdateRelay>();
				relay.OnUpdate.Register( _onUpdate );
			}
			return _onUpdate;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Clear()
	{
		if( services != null )
		{
			foreach( var kvp in services )
			{
				var ser = kvp.Value;
				if( ser is IDisposable disposable ) disposable.Dispose();
				if( ser is ICustomDisposableKill killable ) killable.Kill();
			}
		}		
		// Log( $"{"ServiceLocator".Colorfy(TypeName)}.{"Clear".Colorfy(Verbs)}()" );
		services.Clear();
		genericServices.Clear();
		_onUpdate?.Kill();
		_onUpdate = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void AfterSceneLoad()
	{
		if (services == null) return;
		foreach( var kvp in services )
		{
			var ser = kvp.Value;
			if( ser is IAfterSceneLoad load ) load.AfterSceneLoad();
		}
	}

	public class GenericsComparer : IEqualityComparer<Type[]>
	{
		public bool Equals(Type[] x, Type[] y)
		{
			var xNull = x == null;
			var yNull = y == null;
			if (xNull) return yNull;
			if (yNull) return false;
			if (x.Length != y.Length) return false;
			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i]) return false;
			}

			return true;
		}

		public int GetHashCode(Type[] obj)
		{
			var hash = 0;
			for (int i = 0; i < obj.Length; i++)
			{
				Type type = obj[i];
				hash += type.GetHashCode() * i;
			}

			return hash;
		}

		private GenericsComparer()
		{
		}

		public static readonly GenericsComparer Instance = new GenericsComparer();
	}

	public static void TryGet<T>(ref T serv) where T : IService
	{
		if (serv != null)
		{
        	LogVerbose( $"TryGet<{typeof(T)}>( {serv.ToStringOrNullColored( Colors.Console.Names )} ) already set" );
			return;
		}
		serv = Get<T>();
        LogVerbose( $"TryGet<{typeof(T)}>() returned {serv.ToStringOrNullColored( Colors.Console.Names )}" );
	}

	public static T Get<T>() where T : IService
	{
		return (T)Get(typeof(T));
	}
	

    public static T Request<T>() where T : IService, new()
    {
		var serv = Get<T>();
		if( serv != null ) return serv;
		var newServ = new T();
		Register( newServ );
		if( newServ is IUpdatable up ) OnUpdate.Register( up.Update );
		return newServ;
    }

	public static IService Get(Type type)
	{
		if (type.IsGenericType)
		{
			if (genericServices.TryGetValue(type, out var gServices))
			{
				var generics = type.GetGenericArguments();
				if (gServices.TryGetValue(generics, out var gService)) return gService;
				LogVerbose($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get generic service of type {type.Name.Colorfy(Keyword)}<{generics.Select(g => g.Name.Colorfy(TypeName))}>");
			}

			LogVerbose($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get generic service of type {type.Name.Colorfy(Keyword)}");
		}

		if (services.TryGetValue(type, out var service)) return service;
		LogVerbose($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get service of type {type.Name.Colorfy(Keyword)}");
		return null;
	}

	public static bool Contains(Type type) => services.ContainsKey(type);
	public static bool Contains<T>() => Contains(typeof(T));

	public static void CallOnRegister<T>(Action callback) where T : IService
	{
		if (Contains<T>())
		{
			callback();
			return;
		}

		onServiceRegisteredCallbacks.TryAdd(typeof(T), new List<Action>());

		var list = onServiceRegisteredCallbacks[typeof(T)];
		list.Add(callback);
	}

	public static void DeregisterCallOnRegister<T>(Action callback) where T : IService
	{
		if (!onServiceRegisteredCallbacks.ContainsKey(typeof(T))) return;

		var list = onServiceRegisteredCallbacks[typeof(T)];
		list.Remove(callback);
	}

	public static void CallWhenReady<T>(Action callback) where T : IService
	{
		if (services.TryGetValue(typeof(T), out var service))
		{
			if (service is IReadyService readyService) readyService.IsReady.CallWhenReady(callback);
			else callback();
			return;
		}

		onServiceReadyCallbacks.TryAdd(typeof(T), new List<Action>());

		var list = onServiceReadyCallbacks[typeof(T)];
		list.Add(callback);
	}

	public static void Register(object obj)
	{
		var type = obj.GetType();

		if (obj is not IService service)
		{
			LogVerbose($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Register non-IService object of type {type.Name.Colorfy(Keyword)}");
			return;
		}

		var builder = new StringBuilder();
		builder.AppendLine($"Registering ({type.Name.Colorfy(Keyword)})");

		RegisterConcreteType(service, type, builder);
		RegisterInterfaces(service, type, builder);

		LogVerbose(builder.ToString());

		// if (service is IStartable startable) startable.Start();
	}

	private static void RegisterInterfaces(IService service, Type type, StringBuilder builder)
	{
		foreach (var typeInterface in type.GetInterfaces())
		{
			var isService = typeInterface.Implements(ServType) && typeInterface != ServType;
			if (!isService) continue;

			if (typeInterface.IsGenericType)
			{
				if (!genericServices.TryGetValue(typeInterface, out var gService))
				{
					gService = new Dictionary<Type[], IService>(GenericsComparer.Instance);
					genericServices.Add(typeInterface, gService);
				}

				var generics = typeInterface.GetGenericArguments();
				builder.AppendLine($"{(gService.ContainsKey(generics) ? "REPLACED" : "Registered").Colorfy(Verbs)} as generic {typeInterface.Name.Colorfy(Keyword)}<{string.Join(",", generics.Select(g => g.Name.Colorfy(TypeName)))}>");
				gService[generics] = service;
			}
			else
			{
				builder.AppendLine($"{(services.ContainsKey(typeInterface) ? "REPLACED" : "Registered").Colorfy(Verbs)} as {typeInterface.Name.Colorfy(Keyword)} ");
				ReallyRegister(service, typeInterface);
			}
		}

		foreach (var typeInterface in type.GetInterfaces())
		{
			var isService = typeInterface.Implements(ServType) && typeInterface != ServType;
			if (isService) continue;
			builder.AppendLine($"{"IGNORED".Colorfy(Danger)} as {typeInterface.Name.Colorfy(Keyword)}");
		}
	}

	private static void RegisterConcreteType(IService service, Type type, StringBuilder builder)
	{
		builder.AppendLine($"{(services.ContainsKey(type) ? "REPLACED" : "Registered").Colorfy(Verbs)} as {type.Name.Colorfy(Keyword)}");
		ReallyRegister(service, type);
	}

	private static void ReallyRegister(IService service, Type type)
	{
		services[type] = service;

		if (onServiceRegisteredCallbacks.TryGetValue(type, out var registerList))
		{
			foreach (var callback in registerList) callback.Invoke();
		}

		if (onServiceReadyCallbacks.TryGetValue(type, out var readyList))
		{
			foreach (var callback in readyList)
			{
				if( service is IReadyService readyService ) readyService.IsReady.CallWhenReady(callback);
				else callback();
			}
			onServiceReadyCallbacks.Remove(type);
		}
	}

	public static void Unregister(object obj)
	{
		var type = obj.GetType();
		var service = obj as IService;
		if (service == null)
		{
			LogError($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Unregister non-IService object of type {type.Name.Colorfy(Keyword)}");
			return;
		}

		// _services[type] = service;
		var SB = new StringBuilder();
		SB.AppendLine($"{"ServiceLocator".Colorfy(TypeName)} Unregister( {type.Name.Colorfy(Keyword)} )");
		var servType = typeof(IService);
		foreach (var typeInterface in type.GetInterfaces())
		{
			var isService = typeInterface.Implements(servType) && typeInterface != servType;
			if (!isService) continue;
			if (typeInterface.IsGenericType)
			{
				if (genericServices.TryGetValue(typeInterface, out var gService))
				{
					var generics = typeInterface.GetGenericArguments();
					var removedGenerics = gService.TryGetValue(generics, out var qgServ) && service == qgServ && gService.Remove(generics);
					SB.AppendLine($"{(removedGenerics ? "Unregistered".Colorfy(Verbs) : "NOT_FOUND".Colorfy(Danger))} as generic {typeInterface.Name.Colorfy(Keyword)}<{string.Join(",", generics.Select(g => g.Name.Colorfy(TypeName)))}>");
				}
			}
			else
			{
				var removed = services.TryGetValue(typeInterface, out var qServ) && service == qServ && services.Remove(typeInterface);
				SB.AppendLine($"{(removed ? "Unregistered".Colorfy(Verbs) : "NOT_FOUND".Colorfy(Danger))} as {typeInterface.Name.Colorfy(Keyword)}");
			}
		}

		LogVerbose(SB.ToString());
	}

	[HideInCallstack] private static void LogVerbose( string message ) => K10Log<ServicesLogCategory>.LogVerbose( message );
	[HideInCallstack] private static void LogError(string message) => K10Log<ServicesLogCategory>.Log( LogSeverity.Error, message );
}