public class HasAutomationOperation : ICondition
{
#if UNITY_EDITOR
	public bool Check() => AutomationWindow.HasDataToRun();
#else
	public bool Check() => false;
#endif
}
