using System.Collections.Generic;


public static class FuncBoolExtentions
{
	public static bool And( this IEnumerable<System.Func<bool>> funcs )
	{
		foreach( var f in funcs ) if( !f() ) return false;
		return true;
	}

	public static bool Or( this IEnumerable<System.Func<bool>> funcs )
	{
		foreach( var f in funcs ) if( f() ) return true;
		return false;
	}

	public static bool Not( this System.Func<bool> func ) => !func();
}
