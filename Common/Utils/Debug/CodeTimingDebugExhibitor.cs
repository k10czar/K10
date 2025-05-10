using System;
using UnityEngine;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public abstract class CodeTimingDebugExhibitor : MonoBehaviour
{
	public const int EXECUTION_ORDER = 20000;

	protected float timer;

	[SerializeField] protected float tickInterval = 0.0f;
	[ExtendedDrawer(true),SerializeReference] IEventBinderReference _deepToogle;

	protected abstract void SetLog( string log );
	protected abstract void OnEnableChange( bool enabled );

    void OnEnable()
	{
		CodeTimingDebug.Enable();
		_deepToogle?.Register( TryToggleDeep );
	}

	void OnDisable()
	{
		CodeTimingDebug.Disable();
		_deepToogle?.Unregister( TryToggleDeep );
	}

	void TryToggleDeep()
	{
		CodeTimingDebug.ToogleDeep();
	}

	void LateUpdate()
	{
		var log = CodeTimingDebug.GetLog();
		
		if (tickInterval > Mathf.Epsilon)
		{
			timer += Time.unscaledDeltaTime;
			if (timer > tickInterval)
				timer %= tickInterval;
			else
				return;
		}

		SetLog(log);
		CodeTimingDebug.ClearUnusedData();
	}
}
