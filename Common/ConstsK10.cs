using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class ConstsK10
{
	public const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;


	public const string NULL_STRING = "NULL";

#if UNITY_EDITOR
	// Ever Debug on Unity Editor
	public const string DEBUG_CONDITIONAL = "UNITY_EDITOR";
#else
	public const string DEBUG_CONDITIONAL = "DEBUG";
#endif
}
