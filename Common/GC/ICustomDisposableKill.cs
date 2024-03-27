

//I dont want to mix with IDisposable features, because I cannot predict the results so I did this interface separated
public interface ICustomDisposableKill
{
	void Kill();
}

public static class ICustomDisposableKillExtensions
{
	public static void TryKill( this ICustomDisposableKill self )
	{
		if( self == null ) return;
		self.Kill();
	}

    public static void KillAndClear<T>( this T[] collection ) where T : ICustomDisposableKill
    {
        if( collection == null ) return;
        for( int i = 0; i < collection.Length; i++ ) 
        {
            collection[i]?.Kill();
            collection[i] = default(T);
        }
    }
}