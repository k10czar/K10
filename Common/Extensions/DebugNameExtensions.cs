using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace K10.Common
{
	public static class DebugNameExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameAndType(this Object obj, string nullString = ConstsK10.NULL_STRING) => (obj != null) ? $"{obj.name}<{obj.TypeNameOrNull()}>" : nullString;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameAndTypeColored(this Object obj, string nullString = ConstsK10.NULL_STRING) => (obj != null) ? $"{obj.name.Colorfy(Colors.Console.Names)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameAndTypeColored(this Object obj, Color nameColor, string nullString = ConstsK10.NULL_STRING) => (obj != null) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameAndTypeColored(this Object obj, Color nameColor, Color typeColor, string nullString = ConstsK10.NULL_STRING) => (obj != null) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameAndTypeColored(this Object obj, Color nameColor, Color typeColor, Color nullColor, string nullString = ConstsK10.NULL_STRING) => (obj != null) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(nullColor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameOrNull(this Object obj, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.name : nullString;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string NameOrNullColored(this Object obj) => obj != null ? obj.name : ConstsK10.NULL_STRING_COLORED;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringColored(this bool boolValue) => boolValue.ToString().Colorfy(boolValue ? Colors.Console.Numbers : Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringColored(this object obj, Color valueColor) => obj.ToString().Colorfy(valueColor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringOrNull(this object obj, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.ToString() : nullString;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringOrNullColored(this object obj, Color valueColor, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringOrNullColored(this object obj) => obj != null ? obj.ToString() : ConstsK10.NULL_STRING_COLORED;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringOrNullColored(this object obj, Color valueColor, Color nullColor, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(nullColor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNull(this GameObject obj, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.HierarchyName() : nullString;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNullColored(this GameObject obj, Color valueColor, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNull(this Transform obj, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.HierarchyName() : nullString;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNullColored(this Transform obj, Color valueColor, string nullString = ConstsK10.NULL_STRING) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNull(this Component obj, string nullString = ConstsK10.NULL_STRING) => (obj != null ? (obj.transform.HierarchyName() + $"<{(obj != null ? obj.GetType().ToString() : nullString)}>") : nullString);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HierarchyNameOrNullColored(this Component obj, Color valueColor, string nullString = ConstsK10.NULL_STRING) => (obj != null ? (obj.transform.HierarchyName().Colorfy(valueColor) + $"<{(obj != null ? obj.GetType().ToString() : nullString.Colorfy(Colors.Console.Negation))}>") : nullString.Colorfy(Colors.Console.Negation));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string TypeNameOrNull(this Component target) => target == null ? ConstsK10.NULL_STRING_COLORED : target.GetType().Name;

		public static string ToInspectorName(this object target)
		{
			return target switch
			{
				Object obj => NameOrNullColored(obj),
				string str => string.IsNullOrEmpty(str) ? ConstsK10.NULL_STRING_COLORED : str,
				_ => ToStringOrNullColored(target),
			};
		}

		public static string ToInspectorName<T>(this IReadOnlyList<T> targets, string plural, string empty = null)
		{
			return targets.Count switch
			{
				0 => empty ?? ConstsK10.NULL_STRING_COLORED,
				1 => targets[0].ToInspectorName(),
				2 => $"{targets[0].ToInspectorName()} & {targets[1].ToInspectorName()}",
				_ => $"{targets.Count} {plural}"
			};
		}
	}
}