using System;
using Unity.Mathematics;
using UnityEngine;

public interface IScreenVisibility : IScreenVisibilityObserver, IScreenVisibilitySetter { }

public interface IScreenVisibilityObserver
{
	IBoolStateObserver IsVisible { get; }
}

public interface IScreenVisibilitySetter
{
	Vector3 Position { get; }
	void SetVisibility( bool visible );
}

public class ScreenVisibility : MonoBehaviour, IScreenVisibility
{
	// [SerializeField] Vector3 _viewportPosition;

	private readonly BoolState _isVisible = new BoolState( true );
	public IBoolStateObserver IsVisible => _isVisible;

    public Vector3 Position => transform.position;

    void OnEnable()
	{
		ScreenVisibilityCheckSystem.Add( this );
	}

	void OnDisable()
	{
		ScreenVisibilityCheckSystem.Remove( this );
		_isVisible.SetFalse();
	}

	public void SetVisibility( bool isVisible )
	{
		// _viewportPosition = viewportPosition;
		_isVisible.Setter( isVisible );
	}
}