using UnityEngine;

public interface IUnityEventsRelay
{
	IEventRegister OnDestroy { get; }
	IBoolStateObserver IsActive { get; }
	IBoolStateObserver IsAlive { get; }
	IEventValidator LifetimeValidator { get; }
}

public class GameObjectEventsRelay : MonoBehaviour, IUnityEventsRelay
{
	private bool _destroyed = false;
	private EventSlot<GameObject> _onDestroy;

	private EventSlot<GameObject> _onDisable;
	private EventSlot _onUpdate;

	private BoolState _isActive;
	private BoolState _isAlive;
	private Validator _lifetimeValidator;

	[SerializeField] private bool _triggerEventsOnApplicationQuit = false;

	IEventRegister IUnityEventsRelay.OnDestroy => OnDestroyEvent;

	public IEventRegister<GameObject> OnDestroyEvent => Lazy.Request(ref _onDestroy);
	public IEventRegister<GameObject> Disabled => Lazy.Request(ref _onDisable);

	public IBoolStateObserver IsActive
	{
		get
		{
			if (_isActive == null) _isActive = new BoolState(enabled && gameObject.activeInHierarchy);
			return _isActive;
		}
	}

	public IBoolStateObserver IsAlive
	{
		get
		{
			if (_isAlive == null)
			{
				if (_destroyed) return FalseState.Instance;
				_isAlive = new BoolState(true);
			}

			return _isAlive;
		}
	}

	public IEventValidator LifetimeValidator => _lifetimeValidator ?? NewLifetimeValidator();

	private IEventValidator NewLifetimeValidator()
	{
		if (_destroyed) return NullValidator.Instance;

		_lifetimeValidator = new Validator(this);
		return _lifetimeValidator;
	}

	private void OnDestroy() => Clear();

	private void Clear()
	{
		_destroyed = true;

		if (_triggerEventsOnApplicationQuit || !ApplicationEventsRelay.isQuitting)
		{
			_onDestroy?.Trigger(gameObject);
			_isAlive?.SetFalse();
			_lifetimeValidator?.OnDestroy();
		}

		GcClear.AfterKill(ref _onDestroy);
		GcClear.AfterKill(ref _isAlive);
		GcClear.AfterKill(ref _isActive);
		GcClear.AfterKill(ref _lifetimeValidator);
	}

	void OnEnable() => _isActive?.SetTrue();

	void OnDisable()
	{
		if (!_triggerEventsOnApplicationQuit && ApplicationEventsRelay.isQuitting) return;

		_isActive?.SetFalse();
		_onDisable?.Trigger(gameObject);
	}

	~GameObjectEventsRelay() => Clear();

	private class Validator : IEventValidator, ICustomDisposableKill
	{
		private readonly GameObjectEventsRelay _objRelay;

		bool _destroyed = false;
		System.Func<bool> _currentValidationCheck;
		EventSlot _onVoid;

		bool _lastValidation = true;

		public IEventRegister OnVoid => Lazy.Request(ref _onVoid);

		public Validator(GameObjectEventsRelay objRelay)
		{
			_objRelay = objRelay;
		}

		private bool ValidationCheck()
		{
			if (!_lastValidation) return false;
			_lastValidation = _objRelay != null && _objRelay.transform != null && !_destroyed;
			if (!_lastValidation) OnDestroy();
			return _lastValidation;
		}

		public System.Func<bool> CurrentValidationCheck => _currentValidationCheck ??= ValidationCheck;

		public void Kill()
		{
			_onVoid?.Kill();
			_onVoid = null;
		}

		public void OnDestroy()
		{
			_destroyed = true;
			_lastValidation = false;
			_onVoid?.Trigger();
		}
	}
}