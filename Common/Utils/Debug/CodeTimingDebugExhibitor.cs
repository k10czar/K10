using UnityEngine;

public abstract class CodeTimingDebugExhibitor : MonoBehaviour
{
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
		SetLog( CodeTimingDebug.GetLog() );
		CodeTimingDebug.Clear();
	}
}
