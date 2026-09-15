using System.Runtime.CompilerServices;
using UnityEngine;

public static class UnityStringExtensions
{
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameAndType( this Object obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name}<{obj.TypeNameOrNull()}>" : nullString;

	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameAndTypeColored( this Object obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(Colors.Console.Names)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameAndTypeColored( this Object obj, Color nameColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(Colors.Console.TypeName)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameAndTypeColored( this Object obj, Color nameColor, Color typeColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameAndTypeColored( this Object obj, Color nameColor, Color typeColor, Color nullColor, string nullString = ConstsK10.NULL_STRING )=> ( obj != null ) ? $"{obj.name.Colorfy(nameColor)}<{obj.TypeNameOrNullColored(typeColor)}>" : nullString.Colorfy(nullColor);

	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string NameOrNull( this Object obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.name : nullString;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string DebugNameOrNull(this object obj, string nullString = ConstsK10.NULL_STRING)
	{
		if (obj == null) return nullString;
		if (obj is IDebugName dName) return dName.DebugName;
		var uObj = obj as Object;
		return uObj.NameOrNull(nullString);
	}
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNull( this GameObject obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName() : nullString;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNullColored( this GameObject obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNull( this Transform obj, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName() : nullString;
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNullColored( this Transform obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.HierarchyName().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNull( this Component obj, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ? ( obj.transform.HierarchyName() + $"<{( obj != null ? obj.GetType().ToString() : nullString )}>" ) : nullString );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string HierarchyNameOrNullColored( this Component obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => ( obj != null ? ( obj.transform.HierarchyName().Colorfy(valueColor) + $"<{( obj != null ? obj.GetType().ToString() : nullString.Colorfy(Colors.Console.Negation) )}>" ) : nullString.Colorfy(Colors.Console.Negation) );
}
