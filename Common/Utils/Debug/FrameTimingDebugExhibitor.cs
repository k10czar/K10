using System;
using UnityEngine;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public abstract class FrameTimingDebugExhibitor : MonoBehaviour
{
	public const int EXECUTION_ORDER = 20000;

	protected float timer;

	[SerializeField] protected float tickInterval = 0.0f;
	[ExtendedDrawer(true),SerializeReference] IEventBinderReference _deepToogle;
    FpsCounter _fps;

	protected abstract void SetLog( string log );
	protected abstract void OnEnableChange( bool enabled );

    void OnEnable()
	{
		FrameTimingDebug.Enable();
		_deepToogle?.Register( TryToggleDeep );
        _fps = new FpsCounter( .3333f );
	}

	void OnDisable()
	{
		FrameTimingDebug.Disable();
		_deepToogle?.Unregister( TryToggleDeep );
	}

	void TryToggleDeep()
	{
		FrameTimingDebug.ToogleDeep();
	}

	void LateUpdate()
	{
		_fps.Update();
		var log = FrameTimingDebug.GetLog();
		
		if (tickInterval > Mathf.Epsilon)
		{
			timer += Time.unscaledDeltaTime;
			if (timer > tickInterval)
				timer %= tickInterval;
			else
				return;
		}

		SetLog($"{_fps.CurrentFpsText}\n{log}");
		FrameTimingDebug.ClearUnusedData();
	}
}
