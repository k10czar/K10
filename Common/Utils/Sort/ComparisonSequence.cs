using System;
using System.Collections.Generic;

public class ComparisonSequence<T>
{
    List<Comparison<T>> _sequence = new List<Comparison<T>>();
    Bits _reversion = new Bits();

    public ComparisonSequence( params Comparison<T>[] initialSequence )
    {
        _sequence = new List<Comparison<T>>( initialSequence );
    }

    public int Comparison( T a, T b )
    {
        for( int i = 0; i < _sequence.Count; i++ )
        {
            var comp = _sequence[i];
            var result = comp( a, b );
            if( result == 0 ) continue;
            return _reversion[i] ? -result : result;
        }
        return 0;
    }

    public void Toggle( Comparison<T> comparison )
    {
        var index = _sequence.IndexOf( comparison );
        if( index == -1 )
        {
            for( int i = _sequence.Count - 1; i >= 0; i-- ) _reversion[i+1] = _reversion[i];
            _reversion[0] = false;
            _sequence.Insert( 0, comparison );
            return;
        }
        Toggle( index );
    }

    public void Toggle( int index )
    {
        if( index == 0 )
        {
            _reversion.Flip( 0 );
            return;
        }
        var comparison = _sequence[index];
        var state = _reversion[index];
        for( int i = index; i > 0; i-- ) 
        {
            _reversion[i] = _reversion[i-1];
            _sequence[i] = _sequence[i-1];
        }
        _sequence[0] = comparison;
        _reversion[0] = state;
    }

    public int IndexOf( Comparison<T> comparison ) => _sequence.IndexOf( comparison );
    public bool Reverted( int index ) => _reversion[index];
}
