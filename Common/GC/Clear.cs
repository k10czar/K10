

//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated
public static class Clear
{
	public static void AfterKill<T>( ref T disp ) where T : ICustomDisposableKill
	{
		disp?.Kill();
		Clear.Now( ref disp );
	}

	public static void Now<T>( ref T disp )
	{
		disp = default( T );
	}
}