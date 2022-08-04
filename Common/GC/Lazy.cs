using System.Runtime.CompilerServices;

//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated
public static class Lazy
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static T Request<T>( ref T field ) where T : class, new()
	{
		return field ?? ( field = new T() );
		// if( field == null ) field = new T();
		// return field;
	}
}