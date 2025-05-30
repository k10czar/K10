using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class ConstsK10
{
	public const string NULL_STRING = "NULL";

#if UNITY_EDITOR 
#if EDITOR_WITHOUT_DEBUG 
	public const string CODE_METRICS_CONDITIONAL = "OOOO";
#else
	public const string CODE_METRICS_CONDITIONAL = "UNITY_EDITOR";
#endif
#else
	public const string CODE_METRICS_CONDITIONAL = "DEBUG";
#endif
}
