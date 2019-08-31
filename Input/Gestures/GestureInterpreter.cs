using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class TimedPosition
{
	public TimedPosition( Vector2 pos )
	{
		Position = pos;
		Time = UnityEngine.Time.unscaledTime;
	}

    public Vector2 Position { private set; get; }
	public float Time { private set; get; }
}
	
public class TapGesture
{
	public TapGesture( Vector2 pos )
	{
		Position = pos;
		Time = UnityEngine.Time.unscaledTime;
	}

    public Vector2 Position { private set; get; }
	public float Time { private set; get; }
}
	
public class DoubleTapGesture : TapGesture
{
	public DoubleTapGesture( Vector2 pos, TapGesture first ) : base( pos )
	{
		FirstTap = first;
	}
	
	public TapGesture FirstTap { private set; get; }
}

public class GestureInterpreter : MonoBehaviour
{
	[SerializeField] UnityEngine.UI.Text _debug;
	public float _clickTime = .4f;
	public float _doubleClickTime = .3f;
	public float _clickMaxDist = 15f;
	
	private readonly DragManager _dragManager = new DragManager();
	
	private readonly SubscribersCollection< System.Func< TapGesture, bool > > _tapListeners = new SubscribersCollection<System.Func<TapGesture, bool>>();
//	public static event Func<bool> OnFlick;
//	public static event Func<bool> OnPinch;
//	public static event Func<bool> OnPinchComplete;
	
	private readonly List<TapGesture> _possibleDoubleTaps = new List<TapGesture>();

	public IEnumerable<TapGesture> PossibleDoubleTaps
	{
		get
		{
			_possibleDoubleTaps.RemoveAll( ( t ) => ( Time.unscaledTime - t.Time ) > _doubleClickTime );
			return _possibleDoubleTaps;
		}
	}
	
	public SubscribersCollection<System.Func<TapGesture, bool>> TapEvents { get { return _tapListeners; } }
	public SubscribersCollection<System.Func<DragGesture, bool>> DragEvents { get { return _dragManager.DragEvents; } }	
	public SubscribersCollection<System.Func<PinchGesture, bool>> PinchEvents { get { return _dragManager.PinchEvents; } }	

	void Debug( string s )
	{
		if( _debug == null )
			return;

		_debug.text += "\n" + s;
	}
	
	void Update()
	{
		_dragManager.UncheckedAll();

		#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_FLASH || UNITY_WEBGL
		#region Mouse Interpreter
		if( Input.GetMouseButton( 0 ) )
		{
            Vector2 pos = Input.mousePosition;
            //pos.y = Screen.height - pos.y;
			if( EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject() ) _dragManager.NotifyMouseDrag( pos );
			else _dragManager.CancelMouseDrag( pos );
		}
		#endregion Mouse Interpreter
		#endif
		
		foreach( var touch in Input.touches )
		{
            Vector2 pos = touch.position;
			//pos.y = Screen.height - pos.y;
			if( EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject( touch.fingerId ) ) _dragManager.NotifyDrag( touch.fingerId, pos );
			else _dragManager.CancelDrag( touch.fingerId, pos );
		}
		
		var endedDrags = _dragManager.VerifyUnchecked();
		
		foreach( var drag in endedDrags )
		{
			if( drag.LifeTime <= _clickTime && drag.Distance <= _clickMaxDist )
			{
				TapGesture firstTap = FirstTapIfImDouble( drag.LastPoint );
				TapGesture tap = null;
				
				if( firstTap != null )
				{
					tap = new DoubleTapGesture( drag.LastPoint, firstTap );
					_possibleDoubleTaps.Remove( firstTap );
				}
				else
				{
					tap = new TapGesture( drag.LastPoint );
					_possibleDoubleTaps.Add( tap );
				}
				
				foreach( var listener in TapEvents.Subscribers )
				{
					if( listener( tap ) )
						break;
				}
			}
		}

		_dragManager.VerifyPinch( Debug );
	}
	
	private TapGesture FirstTapIfImDouble( Vector2 position )
	{
		for( int i = _possibleDoubleTaps.Count - 1; i >= 0; i-- )
		{
			var tap = _possibleDoubleTaps[i];
			if( ( Time.unscaledTime - tap.Time ) > _doubleClickTime )
			{
				_possibleDoubleTaps.RemoveAt( i );
			}
			else
			{
				if( Vector2.Distance( tap.Position, position ) <= _clickMaxDist )
				{
					return tap;
				}
			}
		}
		return null;
	}
}
