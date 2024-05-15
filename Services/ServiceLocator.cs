using System.Collections.Generic;
using System.Text;
using UnityEngine;

using static Colors.Console;

public static class ServiceLocator
{
	[ResetedOnLoad] static Dictionary<System.Type, IService> _services = new Dictionary<System.Type, IService>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
		// Debug.Log( $"{"ServiceLocator".Colorfy(TypeName)}.{"Clear".Colorfy(Verbs)}()" );
        _services.Clear();
    }

	public static T Get<T>() where T : IService { return (T)Get(typeof(T)); }

	public static IService Get( System.Type type )
	{
		if (_services.TryGetValue(type, out var service)) return service;
		Debug.Log($"{"ServiceLocator".Colorfy(TypeName)} {"CANNOT".Colorfy(Danger)} Get service of type {type.Name.Colorfy(Keyword)}");
		return null;
	}

	public static bool Contains(System.Type type) => _services.ContainsKey(type);

	public static void Register(IService service) 
	{
		var type = service.GetType();
		// _services[type] = service;
		var SB = new StringBuilder();
		SB.AppendLine($"{"ServiceLocator".Colorfy(TypeName)} Register( {type.Name.Colorfy(Keyword)} )");
		var servType = typeof(IService);
		foreach ( var typeInterface in type.GetInterfaces() )
		{
			var isService = typeInterface.Implements(servType) && typeInterface != servType;
			if (!isService) continue;
			SB.AppendLine($"{"Registered".Colorfy(Verbs)} as {typeInterface.Name.Colorfy(Keyword)}");
			_services[typeInterface] = service;
		}
		foreach ( var typeInterface in type.GetInterfaces() )
		{
			var isService = typeInterface.Implements(servType) && typeInterface != servType;
			if (isService) continue;
			SB.AppendLine($"{"IGNORED".Colorfy(Danger)} as {typeInterface.Name.Colorfy(Keyword)}");
		}
		Debug.Log(SB.ToString());
		if(service is IStartable startable) startable.Start();
	}

	public static void Unregister(IService service)
	{
		var type = service.GetType();
		// _services[type] = service;
		var SB = new StringBuilder();
		SB.AppendLine($"{"ServiceLocator".Colorfy(TypeName)} Unregister( {type.Name.Colorfy(Keyword)} )");
		var servType = typeof(IService);
		foreach (var typeInterface in type.GetInterfaces())
		{
			var isService = typeInterface.Implements(servType) && typeInterface != servType;
			if (!isService) continue;
			var removed = _services.Remove(typeInterface);
			SB.AppendLine($"{(removed ? "Unregistered".Colorfy(Verbs) : "NOT_FOUND".Colorfy(Danger))} as {typeInterface.Name.Colorfy(Keyword)}");
		}
		Debug.Log(SB.ToString());
	}
}
