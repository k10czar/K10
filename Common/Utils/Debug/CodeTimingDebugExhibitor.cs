using UnityEngine;

public abstract class CodeTimingDebugExhibitor : MonoBehaviour
{
	protected float timer;
	[SerializeField] protected float tickRate = 0.5f;

	protected abstract void SetLog( string log );
	protected abstract void OnEnableChange( bool enabled );

	void OnEnable()
	{
		CodeTimingDebug.Enable();
	}

	void OnDisable()
	{
		CodeTimingDebug.Disable();
	}

	void OnPostRender()
	{
		var log = CodeTimingDebug.GetLog();
		timer += Time.unscaledDeltaTime;
		if (timer > tickRate)
			timer %= tickRate;
		else
			return;

		SetLog(log);
		CodeTimingDebug.ClearUnusedData();
	}
}
