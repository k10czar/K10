using System;
using UnityEngine;

[Serializable]
public class ValueInterpolation<T> : IValueInterpolationDataAccess<T>
{
    [SerializeField] int _bufferSize = 10;
    [SerializeReference,ExtendedDrawer] IInterpolationFunc<T> _calculation;
    [BlockEdit,SerializeField] int _currentPackageId = 0;
    [BlockEdit,SerializeField] Package[] _packages = null;
    [BlockEdit,SerializeField] int _idOffset = -1;
    [BlockEdit,SerializeField] double _delta = 0;

    public int BufferSize => _packages?.Length ?? 0;
    public int CurrentPackageId => _currentPackageId;
    public double GetPackageTime( int index ) => _packages[index].time;
    public T GetPackageValue( int index ) => _packages[index].value;

    [Serializable]
    public struct Package
    {
        [BlockEdit,SerializeField] public T value;
        [BlockEdit,SerializeField] public double time;
    }

    public void Start( T startValue, double time )
    {
        _packages = new Package[_bufferSize];
        var data = new Package
        {
            value = startValue,
            time = time
        };
        for ( int i = 0; i < _bufferSize; i++ ) 
        {
            data.time = time - ( ( _bufferSize - i ) % _bufferSize );
            _packages[ i ] = data;
        }
    }

    public void Add( T value, double time, float maxDelay )
    {
        if( _packages == null || _packages.Length < _bufferSize )
        {
            Start( value, time );
            return;
        }
        _currentPackageId = ( _currentPackageId + 1 ) % _bufferSize;
        var data = _packages[_currentPackageId];
        var delayToLastSample = time - data.time;
        if( delayToLastSample < maxDelay ) _packages[( _currentPackageId + 1 ) % _bufferSize] = data;
        data.value = value;
        data.time = time;
        _packages[_currentPackageId] = data;
    }

    public T From( double time )
    {
        var itPackage = _currentPackageId + _bufferSize;
        var lastData = _packages[_currentPackageId];
        _idOffset = 0;
        _delta = time - lastData.time;
        if( lastData.time < time ) 
        {
            // Debug.Log( $"Interpolated.From( {time}, last ) { time - lastData.time }" );
            return lastData.value;
        }
        for( int i = 1; i < _bufferSize; i++ )
        {
            var data = _packages[ ( itPackage - i ) % _bufferSize ];
            if( data.time < time )
            {
                _idOffset = -i;
                _delta = ( time - data.time ) / ( lastData.time - data.time );
                // Debug.Log( $"Interpolated.From( {time}, {i} ) { delta } [ {data.time}, {lastData.time} ]" );
                return _calculation.SafeInterpolate( data.value, lastData.value, (float)_delta );// Vector3.Lerp( data.value, lastData.value, (float)_delta );
            }
            lastData = data;
        }
        // Debug.Log( $"Interpolated.From( {time}, first ) { time - lastData.time }" );
        _idOffset = -_bufferSize;
        return lastData.value;
    }
}
