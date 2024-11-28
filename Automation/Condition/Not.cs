using UnityEngine;

public class Not : ICondition
{
	[ExtendedDrawer, SerializeReference] ICondition _condition;
	public bool Check() => _condition == null || !_condition.Check();
	override public string ToString() => $"!{_condition.ToStringOrNull()}";
}
