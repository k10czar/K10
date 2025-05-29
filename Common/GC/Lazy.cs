using System.Runtime.CompilerServices;
using K10;

//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated
public static class Lazy
{
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static T Request<T>( ref T field ) where T : class, new()
	{
		return field ?? ( field = new T() );
		// if( field == null ) field = new T();
		// return field;
	}
	
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static T RequestPoolable<T>( ref T field ) where T : class, new()
	{
		return field ?? ( field = ObjectPool<T>.Request() );
	}

	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static T Request<T>( ref T field, bool alreadyKilled ) where T : class, new()
	{
		if( alreadyKilled ) return field;
		return Request<T>( ref field );
		// if( field == null ) field = new T();
		// return field;
	}
	
	[MethodImpl(Optimizations.INLINE_IF_CAN)]
	public static T RequestPoolable<T>( ref T field, bool alreadyKilled ) where T : class, new()
	{
		if( alreadyKilled ) return field;
		return RequestPoolable( ref field );
	}
}