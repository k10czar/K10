using System.Collections;
using UnityEngine;

public class FakeArraySizeCounter<T> : System.Collections.Generic.IList<T>
{
	private int _count;

	public FakeArraySizeCounter() { _count = 0; }

	public void ResetCount() { _count = 0; }

	public T this[int index] { get { return default( T ); } set { _count = Mathf.Max( _count, index + 1 ); } }

	public int Count => _count;
	public bool IsReadOnly => true;

	public void Add( T item ) { /*_count++;*/ }
	public void Clear() { }
	public bool Contains( T item ) => false;
	public void CopyTo( T[] array, int arrayIndex ) {  }
	public System.Collections.Generic.IEnumerator<T> GetEnumerator() { yield return default(T); }
	public int IndexOf( T item ) => -1;
	public void Insert( int index, T item ) { this[index] = item; }
	public bool Remove( T item ) => false;
	public void RemoveAt( int index ) { }
	IEnumerator IEnumerable.GetEnumerator() { yield return default(T); }
}
