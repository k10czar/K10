using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISubscribableEvent<T>
{
     void Subscribe( T listener );
     bool Unsubscribe( T listener );
}

public class SubscribersCollection<T> : ISubscribableEvent<T>
{
	private class SubscribeEntry : System.IComparable
	{
		public SubscribeEntry( T listener , int layer )
		{
			Layer = layer;
			Listener = listener;
		}

        public T Listener { private set; get; }
        public int Layer { private set; get; }
		
		public int CompareTo( object obj )
		{
			SubscribeEntry other = (SubscribeEntry)obj;
			
			if( other == null ) return 1;
			
			return other.Layer.CompareTo( Layer );
		}
	}
	
	private readonly List<SubscribeEntry> _entries = new List<SubscribeEntry>();
	
	public IEnumerable<T> Subscribers 
	{ 
		get 
		{ 
			List<T> listeners = new List<T>();
			
			foreach(SubscribeEntry entry in _entries)
			{
				listeners.Add(entry.Listener);
			}
			
			return listeners; 
		} 
	}
	
	public bool HasSubscribers { get { return _entries.Count > 0; } }
	
	public void Subscribe( T listener )
	{
		Subscribe( new SubscribeEntry( listener , 0 ) );
	}
	
	public void Subscribe( T listener, int layer )
	{
		Subscribe( new SubscribeEntry( listener , layer ) );
	}
	
	private void Subscribe( SubscribeEntry entry )
	{
		if( _entries.Contains( entry ) )
			return;
		
		_entries.Add( entry );
		_entries.Sort();
	}
	
	public bool Unsubscribe( T listener )
	{
		for (int i = _entries.Count - 1; i >= 0; --i)
		{
			if (_entries[i].Listener.Equals(listener))
			{
				_entries.RemoveAt(i);	
				return true;
			}
		}
		
		return false;
	}
}
