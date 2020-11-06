using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class HangingFeedback : MonoBehaviour
{
	private static HangingFeedback _instance;

	private static readonly List<Message> _messages = new List<Message>();
	private static StateRequester _hasSomeActive = new StateRequester();
	public static IBoolStateObserver HasSomeActive => _hasSomeActive;

	private readonly CachedReference<Message> _message = new CachedReference<Message>();
	private readonly IntState _activesCount = new IntState();
	private readonly BoolState _isActive = new BoolState();

	protected readonly BoolState _isFullActive = new BoolState();
	public IBoolStateObserver IsFullActive => _isFullActive;

	protected readonly ConditionalEventsCollection _validator = new ConditionalEventsCollection();

	void Awake()
	{
		_instance = this;
		_hasSomeActive.RequestOn( gameObject, _isActive );
		_isActive.Synchronize( this.UntilLifeTime<bool>( ChangeActivity ) );
		ReallyUpdateData();
		_message.Synchronize( OnMessageChange );
	}

	void OnEnable()
	{
		ReallyUpdateData();
	}

	protected virtual void OnMessageChange( Message msg )
	{
		_validator.Void();
		if( msg == null ) return;
		_isFullActive.Synchronize( ( (IValueStateSetter<bool>)msg.IsFullActive ), _validator );
	}

	protected virtual void ChangeActivity( bool isActive )
	{
		_isFullActive.Setter( isActive );
		if( !isActive ) _validator.Void();
	}

	void ReallyUpdateData()
	{
		var wasActive = _isActive.Value;

		_activesCount.Setter( _messages.Count );
		var isActive = _messages.Count > 0;
		_isActive.Setter( isActive );

		_message.ChangeReference( _messages.Count > 0 ? _messages[_messages.Count - 1] : null );
	}

	private static void UpdateData()
	{
		Message oldMessage = null;
		if( _messages.Count > 0 ) oldMessage = _messages[_messages.Count - 1];

		for( int i = _messages.Count - 1; i >= 0; i-- )
		{
			if( _messages[i].IsValid ) continue;
			_messages.RemoveAt( i );
		}

		if( _instance != null ) _instance.ReallyUpdateData();
	}

	public static Message CreateNewMessage( string messageText, IEventTrigger executeBeforeVanish = null )
	{
		var message = new Message( messageText );
		_messages.Add( message );
		if( executeBeforeVanish != null ) message.OnFalseState.Register( executeBeforeVanish );
		message.OnFalseState.Register( UpdateData );
		UpdateData();
		return message;
	}

	public class Message : IBoolStateObserver, IVoidable, IEventTrigger
	{
		public readonly string message;
		private bool _isValid = true;
		private readonly EventSlot _onFalseState = new EventSlot();
		private readonly EventSlot<bool> _onChange = new EventSlot<bool>();

		public IEventRegister OnTrueState => FakeEvent.Instance;
		public IEventRegister OnFalseState => _onFalseState;

		public bool Value => _isValid;
		public bool IsValid => _isValid;

		public IEventRegister<bool> OnChange => _onChange;

		private readonly BoolState _isFullActive = new BoolState();
		public IBoolStateObserver IsFullActive => _isFullActive;

		private CanvasGroup _alphaCanvas;

		public float Alpha => _alphaCanvas.alpha;

		public bool Get() => _isValid;
		void IEventTrigger.Trigger() => Void();

		public Message( string msg )
		{
			this.message = msg;
		}

		public void Void()
		{
			if( !_isValid ) return;
			_isValid = false;
			_onFalseState.Trigger();
			_onChange.Trigger( false );
		}

		public System.Action VoidThen( System.Action act = null )
		{
			return () =>
			{
				if( act != null ) act();
				Void();
			};
		}

		public System.Action<T> VoidThen<T>( System.Action<T> act = null )
		{
			return ( T t ) =>
			{
				if( act != null ) act( t );
				Void();
			};
		}

		public void VoidWhen( params IEventRegister[] events )
		{
			for( int i = 0; i < events.Length; i++ )
			{
				var e = events[i];
				if( e == null ) continue;
				e.Register( this );
			}
		}

		public void VoidAfterSeconds( float seconds )
		{
			ExternalCoroutine.StartCoroutine( VoidAfterSecondsCoroutine( seconds ) );
		}

		private IEnumerator VoidAfterSecondsCoroutine( float seconds )
		{
			yield return new WaitForSecondsRealtime( seconds );
			Void();
		}

		public void SetAlphaCanvas( CanvasGroup alpha )
		{
			_alphaCanvas = alpha;
		}
	}
}