using System;
using UnityEngine;

public class LazyArrayOfValue<T> where T : struct
{
	const int DEFAULT_ARRAY_SIZE = 16;
	const float GROWTH_FACTOR = 2;

	T[] _elements = null;
	int _elementsCount = 0;
	float _growthFactor = 2;
	bool _isDirty = false;
	int _dirtyElementsPrediction = 0;

	public int Count => _elementsCount;
	public int PredictedCount => _elementsCount - _dirtyElementsPrediction;

    private T _nullValue;

	public LazyArrayOfValue( T nullValue, int initialSize = DEFAULT_ARRAY_SIZE, float growthFactor = GROWTH_FACTOR )
	{
        _nullValue = nullValue;
		_elements = new T[initialSize];
        for( int i = 0; i < initialSize; i++ ) _elements[i] = nullValue;
		_growthFactor = growthFactor;
	}

	public void GetArrayToIterate( out T[] elements, out int elementsCount )
	{
		if( _isDirty ) RemoveNulls();
		elementsCount = _elementsCount;
		elements = _elements;
	}

	public void RemoveNulls()
	{
		int nextIterator = 0;
		int lastValidElement = -1;
		bool stillHasElementsToLook = false;
		for( int i = 0; stillHasElementsToLook && i < _elementsCount; i++ )
		{
			var element = _elements[i];
			if( !element.Equals( _nullValue ) ) 
			{
				lastValidElement = i;
				continue;
			}
			if( nextIterator <= i ) nextIterator = i + 1;
			for( int j = nextIterator; j < _elementsCount; j++ )
			{
				if( element.Equals( _nullValue ) ) continue;
				nextIterator = j + 1;
				_elements[i] = _elements[j];
				lastValidElement = i;
				stillHasElementsToLook = nextIterator < _elementsCount;
				break;
			}
		}
		_elementsCount = lastValidElement + 1;
		_isDirty = false;
		_dirtyElementsPrediction = 0;
	}

	public bool AddIfNotContains( T newElement )
	{
		if( Contains( newElement ) ) return false;
		Add( newElement );
		return true;
	}

	public bool Contains( T newElement )
	{
		if( newElement.Equals( _nullValue ) ) return false;
		for( int i = 0; i < _elementsCount; i++ ) 
		{
			var element = _elements[i];
			if( !element.Equals( _nullValue ) && element.Equals( newElement ) ) return true;
		}
		return false;
	}

    public void Clear( bool lazy = true )
    {
        if( !lazy )
        {
            for( int i = 0; i < _elementsCount; i++ )
            {
                _elements[i] = _nullValue;
            }
        }
        _elementsCount = 0;
    }

	public void Add( T newElement )
	{
		if( newElement.Equals( _nullValue ) ) return;
		if( _elementsCount >= _elements.Length )
		{
			if( _isDirty ) RemoveNulls();
			if( _elementsCount >= _elements.Length )
			{
				var oldArray = _elements;
				var len = oldArray.Length;
				var newSize = UnityEngine.Mathf.CeilToInt( len * _growthFactor );
				if( newSize == len ) newSize = len + 1;
				Debug.Log( $"LazyArrayOfComponent<<color=lime>{typeof(T).Name}</color>> buffer grow from <color=red>{oldArray.Length}</color> to <color=yellow>{newSize}</color>, you should consider to initialize already with a bigger values to not create new array allocation" );
				_elements = new T[ newSize ];
				for( int i = 0; i < _elementsCount; i++ ) _elements[i] = oldArray[i];
			}
		}
		_elements[_elementsCount] = newElement;
		_elementsCount++;
	}

	public bool LazyRemove( T newElement )
	{
		if( newElement.Equals( _nullValue ) ) return false;
		for( int i = 0; i < _elementsCount; i++ )
		{
			var element = _elements[i];
			if( element.Equals( _nullValue ) ) continue;
			if( !element.Equals( newElement ) ) continue;
			_elements[i] = _nullValue;
			_isDirty = true;
			_dirtyElementsPrediction++;
			return true;
		}
		return false;
	}

	public bool LazyRemoveAt( int id )
	{
		if( id >= _elementsCount ) return false;
		if( _elements[id].Equals( _nullValue ) ) return false;
		_elements[id] = _nullValue;
		_isDirty = true;
		_dirtyElementsPrediction++;
		return true;
	}
}