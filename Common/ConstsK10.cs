using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class ConstsK10
{
	public const string NULL_STRING = "NULL";

#if UNITY_EDITOR
	// Ever Debug on Unity Editor
	public const string CODE_METRICS_CONDITIONAL = "UNITY_EDITOR";
#else
	public const string CODE_METRICS_CONDITIONAL = "CODE_METRICS";
#endif
}
