using UnityEngine;

public class HasAutomationOperation : ICondition
{
#if UNITY_EDITOR
	public bool Check() => AutomationWindow.HasDataToRun();
#else
	public bool Check() => false;
#endif
}

public class HasService : ICondition
{
    [TypeFilter(typeof(IService))]
	[SerializeField] SerializableType serviceType;

	public bool Check() => ServiceLocator.Contains( serviceType );
}
