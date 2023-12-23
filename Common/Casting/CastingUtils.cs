using UnityEngine;

public static class CastingExtensions
{
    public static bool TrySetOn<T>( this object obj, ref T t, bool logFailAsError = false )
    {
        if( TrySetFromString<T>( obj, ref t, logFailAsError ) ) return true;
        if( TrySetFromObject<T>( obj, ref t, logFailAsError ) ) return true;
        return false;
    }

    public static bool TrySetOn<T,K>( this object obj, ref (T,K) tp, bool logFailAsError = false ) => TrySetOn<T,K>( obj, ref tp.Item1, ref tp.Item2, logFailAsError );
    public static bool TrySetOn<T,K>( this object obj, ref T t, ref K k, bool logFailAsError = false )
    {
        if( TrySetFromTuple<T,K>( obj, ref t, ref k, logFailAsError ) ) return true;
        if( TrySetFromStringTuple<T,K>( obj, ref t, ref k, logFailAsError ) ) return true;
        if( TrySetFromObjectTuple<T,K>( obj, ref t, ref k, logFailAsError ) ) return true;
        return false;
    }

    private static bool TrySetFromObject<T>( this object obj, ref T t, bool logFailAsError = false )
    {
        if( obj is T tt )
        {
            t = tt;
            return true;
        }
        if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {typeof(T).FullName.Colorfy( Colors.Console.Types )} the {"object".Colorfy( Colors.Console.Types )}: {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)}" );
        return false;
    }

    private static bool TrySetFromString<T>( this object obj, ref T t, bool logFailAsError = false )
    {
        if( obj is string str )
        {
            var convT = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            if( convT == null ) return false;
            try
            {
                var st = (T)convT.ConvertFromString( str );
                t = st;
                return true;
            }
            catch( System.Exception exTK ) { if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {"( object, object )".Colorfy( Colors.Console.Types )} = ( {str.ToStringOrNull().Colorfy( Colors.Console.Numbers)} )\n{"exception".Colorfy( Colors.Console.Negation )} trying cast as {typeof(T).FullName.Colorfy( Colors.Console.Types )}: {exTK.Message}" ); }
            return false;
        }
        return false;
    }

    private static bool TrySetFromTuple<T,K>( this object obj, ref T t, ref K k, bool logFailAsError = false )
    {
        if( obj is ( T tt, K tk ) )
        {
            k = tk;
            t = tt;
            return true;
        }
        if( typeof( T ) == typeof( K ) )
        {
            if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )} the {"object".Colorfy( Colors.Console.Types )}: {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)}" );
            return false;
        }

        if( obj is ( K itk, T itt ) )
        {
            k = itk;
            t = itt;
            return true;
        }
        if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )} and {$"( {typeof(K).FullName}, {typeof(T).FullName} )".Colorfy( Colors.Console.Types )} the {"object".Colorfy( Colors.Console.Types )}: {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)}" );
        return false;
    }

    private static bool TrySetFromObjectTuple<T,K>( this object obj, ref T t, ref K k, bool logFailAsError = false )
    {
        if( obj is ( object o1, object o2 ) )
        {
            string exTkLog = string.Empty;
            try
            {
                var ot = (T)o1;
                var ok = (K)o2;
                t = ot;
                k = ok;
                return true;
            }
            catch( System.Exception exTK ) { if( logFailAsError ) exTkLog = $"{"exception".Colorfy( Colors.Console.Negation )} trying cast as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )}:{exTK.Message}"; }
            
            if( typeof( T ) == typeof( K ) )
            {
                if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )} the {"( object, object )".Colorfy( Colors.Console.Types )}: {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)}\n{exTkLog}" );
                return false;
            }

            string exKtLog = string.Empty;
            try
            {
                var ot = (T)o2;
                var ok = (K)o1;
                t = ot;
                k = ok;
                return true;
            }
            catch( System.Exception exKT ) { if( logFailAsError ) exKtLog = $"{"exception".Colorfy( Colors.Console.Negation )} trying cast as {$"( {typeof(K).FullName}, {typeof(T).FullName} )".Colorfy( Colors.Console.Types )}: {exKT.Message}"; }
            if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {"( object, object )".Colorfy( Colors.Console.Types )} = ( {o1.ToStringOrNull().Colorfy( Colors.Console.Numbers)}, {o2.ToStringOrNull().Colorfy( Colors.Console.Numbers)} )\n{exTkLog}\n{exKtLog}" );
            return false;
        }
        if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to cast {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)} as {"( object, object )".Colorfy( Colors.Console.Types )}" );
        return false;
    }

    private static bool TrySetFromStringTuple<T,K>( this object obj, ref T t, ref K k, bool logFailAsError = false )
    {
        if( obj is ( string s1, string s2 ) )
        {
            var convT = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            if( convT == null ) return false;
            var convK = System.ComponentModel.TypeDescriptor.GetConverter(typeof(K));
            if( convK == null ) return false;
            string exTkLog = string.Empty;
            try
            {
                var st = (T)convT.ConvertFromString( s1 );
                var sk = (K)convK.ConvertFromString( s2 );
                t = st;
                k = sk;
                return true;
            }
            catch( System.Exception exTK ) { if( logFailAsError ) exTkLog = $"{"exception".Colorfy( Colors.Console.Negation )} trying cast as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )}:{exTK.Message}"; }
            
            if( typeof( T ) == typeof( K ) )
            {
                if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {$"( {typeof(T).FullName}, {typeof(K).FullName} )".Colorfy( Colors.Console.Types )} the {"( string, string )".Colorfy( Colors.Console.Types )}: {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)}\n{exTkLog}" );
                return false;
            }

            string exKtLog = string.Empty;
            try
            {
                var st = (T)convT.ConvertFromString( s2 );
                var sk = (K)convK.ConvertFromString( s1 );
                t = st;
                k = sk;
                return true;
            }
            catch( System.Exception exKT ) { if( logFailAsError ) exKtLog = $"{"exception".Colorfy( Colors.Console.Negation )} trying cast as {$"( {typeof(K).FullName}, {typeof(T).FullName} )".Colorfy( Colors.Console.Types )}: {exKT.Message}"; }
            if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to parse as {"( object, object )".Colorfy( Colors.Console.Types )} = ( {s1.ToStringOrNull().Colorfy( Colors.Console.Numbers)}, {s2.ToStringOrNull().Colorfy( Colors.Console.Numbers)} )\n{exTkLog}\n{exKtLog}" );
            return false;
        }
        if( logFailAsError ) Debug.LogError( $"{"Fail".Colorfy( Colors.Console.Negation)} to cast {obj.ToStringOrNull().Colorfy( Colors.Console.Numbers)} as {"( string, string )".Colorfy( Colors.Console.Types )}" );
        return false;
    }
}
