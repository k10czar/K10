using System;

namespace K10Attributes
{
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public class SceneSelectorAttribute : UnityEngine.PropertyAttribute
	{
	}
}