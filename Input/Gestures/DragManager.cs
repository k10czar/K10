using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PinchGesture
{
	bool _valid = true;
	public bool IsValid { get { return _valid; } }

	EventSlot<PinchGesture> _onPinchComplete = new EventSlot<PinchGesture>();
	EventSlot<PinchGesture> _onPinch = new EventSlot<PinchGesture>();
//	EventSlot<float> _onPinchAngleChanged = new EventSlot<float>();
//	EventSlot<float> _onPinchDistanceChanged = new EventSlot<float>();
	
	public IEventRegister<PinchGesture> OnPinchComplete { get { return _onPinchComplete; } }
	public IEventRegister<PinchGesture> OnPinch { get { return _onPinch; } }
//	public IEventRegister<float> OnPinchAngleChanged { get { return _onPinchAngleChanged; } }
//	public IEventRegister<float> OnPinchDistanceChanged { get { return _onPinchDistanceChanged; } }

	DragGesture _first;
	DragGesture _second;
	
	int _firstStartID;
	int _second2StartID;
	
	float _initialAngle;
	float _initialDistance;
	
	public float InitialAngle { get { return _initialAngle; } }
	public float InitialDistance { get { return _initialDistance; } }
	
	float _lastAngle;
	float _lastDistance;

	public float Angle { get { return _lastAngle; } }
	public float Distance { get { return _lastDistance; } }
	
	float _angleChange;
	float _distanceChange;
	
	public float AngleChange { get { return _angleChange; } }
	public float DistanceChange { get { return _distanceChange; } }

	public PinchGesture( DragGesture first, DragGesture second )
	{
		this._first = first;
		this._second = second;
		
		_firstStartID = first.Points.Count - 1;
		_firstStartID = second.Points.Count - 1;
		
		var pf = first.LastPoint;
		var ps = second.LastPoint;

		_initialAngle = CalculateAngle();
		_initialDistance = CalculateDistance();

		_lastAngle = _initialAngle;
		_lastDistance = _initialDistance;

		first.OnDragComplete.Register( TryDestroy );
	}

	public bool Has( DragGesture fisrt, DragGesture second )
	{
		return fisrt != null && second != null && ( ( _first == fisrt && _second == second ) || ( _first == second && _second == fisrt ) );
	}

	public void TryDestroy()
	{
		if( _valid )
		{
			_valid = false;
			_onPinchComplete.Trigger( this );
		}
	}
	
	float CalculateAngle()
	{
		var pf = _first.LastPoint;
		var ps = _second.LastPoint;
		return Mathf.Atan2( ps.x - pf.x, ps.y - pf.y );
	}
	
	float CalculateDistance() { return Vector2.Distance( _first.LastPoint, _second.LastPoint ); }

	public void Update()
	{
		if( !_valid )
			return;

		var angle = CalculateAngle();
		var distance = CalculateDistance();

		if( _lastAngle != angle )
		{
			_angleChange = angle - _lastAngle;
			if( _angleChange > Mathf.PI ) _angleChange = _angleChange - 2 * Mathf.PI;
			if( _angleChange < -Mathf.PI ) _angleChange = _angleChange + 2 * Mathf.PI;

			_lastAngle = angle;
//			_onPinchAngleChanged.Trigger( angle );
		}
		
		if( _lastDistance != distance )
		{
			_distanceChange = distance - _lastDistance;
			_lastDistance = distance;
//			_onPinchDistanceChanged.Trigger( _distanceChange );
		}

		_onPinch.Trigger( this );
	}
}


public class DragGesture
{
	EventSlot<DragGesture> _onDragComplete = new EventSlot<DragGesture>();
	EventSlot<DragGesture> _onDragCancel = new EventSlot<DragGesture>();
	EventSlot<DragGesture> _onDrag = new EventSlot<DragGesture>();

	public IEventRegister<DragGesture> OnDragComplete { get { return _onDragComplete; } }
	public IEventRegister<DragGesture> OnDrag { get { return _onDrag; } }
	public IEventRegister<DragGesture> OnDragCancel { get { return _onDragCancel; } }

	List<TimedPosition> _points = new List<TimedPosition>();

	public bool IsComplete { private set; get; }
	public bool IsCanceled { private set; get; }
	public IList<TimedPosition> Points { get { return _points; } } //Should be IReadOnlyList
	
	public float LifeTime { get { return Last.Time - First.Time; } }
	public TimedPosition First { get { return _points.First(); } }
	public TimedPosition Last { get { return _points.Last(); } }
	public Vector2 FirstPoint { get { return First.Position; } }
	public Vector2 LastPoint { get { return Last.Position; } }
	
	public float Distance { get { return Vector2.Distance( LastPoint, FirstPoint ); } }
	
	public Vector2 DeltaChange { get { return _points[ _points.Count - 1 ].Position - _points[ _points.Count - 2 ].Position; } }
	public Vector2 Delta { get { return LastPoint - FirstPoint; } }
	
	public DragGesture( Vector2 beginPoint )
	{
		_points.Add( new TimedPosition( beginPoint ) );
	}
	
	#region Damn C#
	// Want this methods to be acessed only from CGestureInterpreter but C# Dont let me do
	public void Simplify()
	{
		_points = new List<TimedPosition>{ First, Last };
	}
	
	internal void AddPoint( Vector2 point )
	{
		if( point != LastPoint )
		{
			_points.Add( new TimedPosition( point ) );
			_onDrag.Trigger( this );
		}
	}
	
	internal void CompleteGesture()
	{
		IsComplete = true;
		_points.Add( new TimedPosition( LastPoint ) );
		_onDragComplete.Trigger( this );
	}

	internal void CancelGesture()
	{
		IsCanceled = true;
		_onDragCancel.Trigger( this );
		CompleteGesture();
    }
	#endregion Damn C#
}

public class DragManager
{
	Dictionary< int, DragGesture > _currentDrags = new Dictionary<int, DragGesture>();
	PinchGesture _currentPinch;
	
	SubscribersCollection<System.Func<DragGesture, bool>> _listeners = new SubscribersCollection<System.Func<DragGesture, bool>>();
	SubscribersCollection<System.Func<PinchGesture, bool>> _pinchListeners = new SubscribersCollection<System.Func<PinchGesture, bool>>();

	List<int> _toRemove = new List<int>();
	
	public SubscribersCollection<System.Func<DragGesture, bool>> DragEvents { get { return _listeners; } }
	public SubscribersCollection<System.Func<PinchGesture, bool>> PinchEvents { get { return _pinchListeners; } }
	
	public bool IsNecessary { get { return _listeners.HasSubscribers; } }
	
	public void UncheckedAll()
	{
		_toRemove.Clear();
		_toRemove.AddRange( _currentDrags.Keys );
	}
	
	public void VerifyPinch( System.Action<string> Debug )
	{
		if( _currentPinch != null && !_currentPinch.IsValid )
		{
			Debug( "Old Pinch was Invalid" );
			_currentPinch = null;
		}

		if( _currentDrags.Count == 2 )
		{
			Debug( "2 drags " + Time.unscaledTime );
			var values = _currentDrags.Values.GetEnumerator();
			values.MoveNext();
			var first = values.Current;
			values.MoveNext();
			var second = values.Current;

			if( _currentPinch != null && !_currentPinch.Has( first, second ) )
			{
				Debug( "Old Pinch was Invalid(2)" );
				_currentPinch.TryDestroy();
				_currentPinch = null;
			}

			if( _currentPinch == null )
			{
				Debug( "Create new Pinch " + ( first != null ) + " " + ( second != null ) );
				_currentPinch = new PinchGesture( first, second );
				Trigger( _pinchListeners, _currentPinch );
			}
			else
			{
				_currentPinch.Update();
			}
		}
		else if( _currentPinch != null )
		{
			Debug( "Old Pinch was Invalid(3)" );
			_currentPinch.TryDestroy();
			_currentPinch = null;
		}
	}

	void Trigger<T>( SubscribersCollection< System.Func<T, bool> > listeners, T val )
	{
		foreach( var listener in listeners.Subscribers )
		{
			if( listener( val ) )
				break;
		}
	}
	
	public List<DragGesture> VerifyUnchecked()
	{
		var ret = ( from d in _toRemove select _currentDrags[ d ] ).ToList();
		foreach( var key in _toRemove )
		{
			CompleteDrag( key );
		}
		return ret;
	}
	
	public void CompleteMouseDrag() { CompleteDrag( 0 ); }
	void CompleteDrag( int dragId )
	{
		_currentDrags[ dragId ].CompleteGesture();
		_currentDrags.Remove( dragId );
	}
	
	void NewDragEvent( int dragId, DragGesture gesture )
	{
		_currentDrags[ dragId ] = gesture;
		Trigger( _listeners, gesture );
	}

	public void CancelMouseDrag( Vector2 position ) { CancelDrag( 0, position ); }
	public void CancelDrag( int fingerId, Vector2 position )
	{
		int id = FingerIdToDragId( fingerId );
		DragGesture gest;
		if( _currentDrags.TryGetValue( id, out gest ) )
		{
			gest.CancelGesture();
		}
		_toRemove.Remove( id );
	}

	public void NotifyMouseDrag( Vector2 position ) { NotifyDrag( 0, position ); }
	public void NotifyDrag( int fingerId, Vector2 position )
	{
		int id = FingerIdToDragId( fingerId );
		DragGesture gest;
		if( _currentDrags.TryGetValue( id, out gest ) )
		{
			gest.AddPoint( position );
		}
		else
		{
			NewDragEvent( id, new DragGesture( position ) );
		}
		
		_toRemove.Remove( id );
	}
	
	int FingerIdToDragId( int fingerId ) { return fingerId + 1; }
	int DragIdToFingerId( int dragId ) { return dragId - 1;	}
}
