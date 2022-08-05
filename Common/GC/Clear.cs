using System.Runtime.CompilerServices;


//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated

public static class Clear
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void AfterKill<T>( ref T data ) where T : ICustomDisposableKill
	{
		data?.Kill();
		Clear.Now( ref data );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void AfterListClear<T>( ref T data ) where T : System.Collections.IList
	{
		data?.Clear();
		Clear.Now( ref data );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Now<T>( ref T data )
	{
		data = default( T );
	}
}

// Another end point for scopes hiding Clear keyword
public static class GcClear
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void AfterKill<T>( ref T data ) where T : ICustomDisposableKill => Clear.AfterKill<T>( ref data );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Now<T>( ref T data ) => Clear.Now<T>( ref data );
}