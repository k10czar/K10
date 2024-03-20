using UnityEngine;

[DefaultExecutionOrder(CodeTimingDebugExhibitor.EXECUTION_ORDER)]
public abstract class CodeTimingDebugExhibitor : MonoBehaviour
{
	public const int EXECUTION_ORDER = 20000;

	protected float timer;

	[SerializeField] protected float tickRate = 0.0f;

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
		
		if (tickRate > Mathf.Epsilon)
		{
			timer += Time.unscaledDeltaTime;
			if (timer > tickRate)
				timer %= tickRate;
			else
				return;
		}

		SetLog(log);
		CodeTimingDebug.ClearUnusedData();
	}
}
