public interface ICondition
{
	bool Check();
}

public static class ConditionExtension
{
	public static bool SafeCheck( this ICondition moment ) => moment?.Check() ?? true;
}
