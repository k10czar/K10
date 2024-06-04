using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronFeather.IronDebug;
using UnityEngine;

using static Colors.Console;

public static class ServiceLocator
{
	private static readonly Type ServType = typeof(IService);

	[ResetedOnLoad] static Dictionary<System.Type, IService> _services = new();
	[ResetedOnLoad] static Dictionary<System.Type, Dictionary<System.Type[], IService>> _genericServices = new();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Clear()
	{
		// Log( $"{"ServiceLocator".Colorfy(TypeName)}.{"Clear".Colorfy(Verbs)}()" );
		_services.Clear();
		_genericServices.Clear();
	}

	public class GenericsComparer : IEqualityComparer<System.Type[]>
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

	public static T Get<T>() where T : IService
	{
		return (T)Get(typeof(T));
	}

	public static IService Get(System.Type type)
	{
		if (type.IsGenericType)
		{
			if (_genericServices.TryGetValue(type, out var gServices))
			{
				var generics = type.GetGenericArguments();
				if (gServices.TryGetValue(generics, out var gService)) return gService;
				Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get generic service of type {type.Name.Colorfy(Keyword)}<{generics.Select(g => g.Name.Colorfy(TypeName))}>");
			}

			Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get generic service of type {type.Name.Colorfy(Keyword)}");
		}

		if (_services.TryGetValue(type, out var service)) return service;
		Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get service of type {type.Name.Colorfy(Keyword)}");
		return null;
	}

	public static bool Contains(System.Type type) => _services.ContainsKey(type);
	public static bool Contains<T>() => Contains(typeof(T));

	public static void Register(object obj)
	{
		var type = obj.GetType();

		if (obj is not IService service)
		{
			Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Register non-IService object of type {type.Name.Colorfy(Keyword)}", true);
			return;
		}

		var builder = new StringBuilder();
		builder.AppendLine($"Registering ({type.Name.Colorfy(Keyword)})");

		RegisterConcreteType(service, type, builder);
		RegisterInterfaces(service, type, builder);

		Log(builder.ToString());

		if (service is IStartable startable) startable.Start();
	}

	private static void RegisterInterfaces(IService service, Type type, StringBuilder builder)
	{
		foreach (var typeInterface in type.GetInterfaces())
		{
			var isService = typeInterface.Implements(ServType) && typeInterface != ServType;
			if (!isService) continue;

			if (typeInterface.IsGenericType)
			{
				if (!_genericServices.TryGetValue(typeInterface, out var gService))
				{
					gService = new Dictionary<Type[], IService>(GenericsComparer.Instance);
					_genericServices.Add(typeInterface, gService);
				}

				var generics = typeInterface.GetGenericArguments();
				builder.AppendLine($"{(gService.ContainsKey(generics) ? "REPLACED" : "Registered").Colorfy(Verbs)} as generic {typeInterface.Name.Colorfy(Keyword)}<{string.Join(",", generics.Select(g => g.Name.Colorfy(TypeName)))}>");
				gService[generics] = service;
			}
			else
			{
				builder.AppendLine($"{(_services.ContainsKey(typeInterface) ? "REPLACED" : "Registered").Colorfy(Verbs)} as {typeInterface.Name.Colorfy(Keyword)} ");
				_services[typeInterface] = service;
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
		builder.AppendLine($"{(_services.ContainsKey(type) ? "REPLACED" : "Registered").Colorfy(Verbs)} as {type.Name.Colorfy(Keyword)}");
		_services[type] = service;
	}

	public static void Unregister(object obj)
	{
		var type = obj.GetType();
		var service = obj as IService;
		if (service == null)
		{
			Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Unregister non-IService object of type {type.Name.Colorfy(Keyword)}", true);
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
				if (_genericServices.TryGetValue(typeInterface, out var gService))
				{
					var generics = typeInterface.GetGenericArguments();
					var removedGenerics = gService.TryGetValue(generics, out var qgServ) && service == qgServ && gService.Remove(generics);
					SB.AppendLine($"{(removedGenerics ? "Unregistered".Colorfy(Verbs) : "NOT_FOUND".Colorfy(Danger))} as generic {typeInterface.Name.Colorfy(Keyword)}<{string.Join(",", generics.Select(g => g.Name.Colorfy(TypeName)))}>");
				}
			}
			else
			{
				var removed = _services.TryGetValue(typeInterface, out var qServ) && service == qServ && _services.Remove(typeInterface);
				SB.AppendLine($"{(removed ? "Unregistered".Colorfy(Verbs) : "NOT_FOUND".Colorfy(Danger))} as {typeInterface.Name.Colorfy(Keyword)}");
			}
		}

		Log(SB.ToString());
	}

	private static void Log(string message, bool isError = false) => IronLog.Log(GameSystem.Services, isError ? LogSeverity.Danger : LogSeverity.Info, message);
}