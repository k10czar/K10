using System.Runtime.CompilerServices;

public static class ConstsK10
{
	public const MethodImplOptions AggrInline = MethodImplOptions.AggressiveInlining;


	public const string NULL_STRING = "NULL";
	public static readonly string NULL_STRING_COLORED = $"<color={Colors.Console.Danger.ToHexRGB()}>NULL</color>";
}