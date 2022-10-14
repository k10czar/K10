using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IUpdatableOnDemand
{
	bool Update( float delta ); // Respond true if need more Update, false if not
}

public interface IUpdaterOnDemand
{
	bool RequestUpdate( IUpdatableOnDemand updateRequester );
}

public class UpdaterOnDemandBehaviour : MonoBehaviour, IUpdaterOnDemand
{
	HashSet<IUpdatableOnDemand> _updating = new HashSet<IUpdatableOnDemand>();

	public void OnDestroy()
	{
		_updating?.Clear();
		StopAllCoroutines();
	}

	public bool RequestUpdate( IUpdatableOnDemand updateRequester )
	{
		var contains = _updating.Contains( updateRequester );
		if( !contains ) StartCoroutine( UpdateCoroutine( updateRequester ) );
		return !contains;
	}

	IEnumerator UpdateCoroutine( IUpdatableOnDemand updateRequester )
	{
		if( !_updating.Contains( updateRequester ) )
		{
			_updating.Add( updateRequester );
			yield return null;
			while( updateRequester.Update( Time.deltaTime ) ) { yield return null; }
			_updating.Remove( updateRequester );
		}
	}
}

public class UpdaterOnDemand : IUpdaterOnDemand
{
	MonoBehaviour _behaviour;
	HashSet<IUpdatableOnDemand> _updating = new HashSet<IUpdatableOnDemand>();
	private Coroutine _courotine;

	public UpdaterOnDemand( MonoBehaviour behaviour, IEventValidator validator = null )
	{
		_behaviour = behaviour;
		if( validator != null ) validator.OnVoid.Register( new CallOnce( Kill ) );
	}

	protected virtual void Kill()
	{
		if( _courotine != null ) _behaviour?.StopCoroutine( _courotine );
		_behaviour = null;
		_updating = null;
	}

	public bool RequestUpdate( IUpdatableOnDemand updateRequester )
	{
		var contains = _updating.Contains( updateRequester );
		if( _behaviour != null && _behaviour.gameObject != null && _behaviour.gameObject.activeInHierarchy && _behaviour.enabled )
		{
			_courotine = _behaviour.StartCoroutine( UpdateCoroutine( updateRequester ) );
		}
		return !contains;
	}

	IEnumerator UpdateCoroutine( IUpdatableOnDemand updateRequester )
	{
		if( !_updating.Contains( updateRequester ) )
		{
			_updating.Add( updateRequester );
			while( _behaviour != null && updateRequester.Update( Time.deltaTime ) ) { yield return null; }
			_updating.Remove( updateRequester );
		}
	}
}

public class ManualUpdaterOnDemand : IUpdaterOnDemand
{
	MonoBehaviour _behaviour;
	HashSet<IUpdatableOnDemand> _updatingHash = new HashSet<IUpdatableOnDemand>();
	List<IUpdatableOnDemand> _updating = new List<IUpdatableOnDemand>();

	public ManualUpdaterOnDemand( MonoBehaviour behaviour )
	{
		_behaviour = behaviour;
	}

	public void Kill()
	{
		_behaviour = null;
		_updatingHash?.Clear();
		_updating?.Clear();
	}

	public bool RequestUpdate( IUpdatableOnDemand updateRequester )
	{
		var contains = _updatingHash.Contains( updateRequester );
		if( !contains )
		{
			_updating.Add( updateRequester );
			_updatingHash.Add( updateRequester );
		}
		return !contains;
	}

	public void Update() { Update( Time.deltaTime ); }
	public void Update( float deltaTime )
	{
		for( int i = _updating.Count - 1; i >= 0; i-- )
		{
			var up = _updating[i];
			if( !up.Update( deltaTime ) )
			{
				_updating.RemoveAt( i );
				_updatingHash.Remove( up );
			}
		}
	}
}
