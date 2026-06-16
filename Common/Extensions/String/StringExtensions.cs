using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class StringExtensions
{	
    static readonly System.Text.RegularExpressions.Regex _colorTagRegex = new( @"<color[^>]*>|</color>", System.Text.RegularExpressions.RegexOptions.Compiled );
    
	[MethodImpl(Optimizations.INLINE_IF_CAN)] public static string ToStringColored( this bool boolValue ) => boolValue.ToString().Colorfy( boolValue ? Colors.Console.Numbers : Colors.Console.Negation );
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string ToStringColored( this object obj, Color valueColor ) => obj.ToString().Colorfy(valueColor);
    [MethodImpl(Optimizations.INLINE_IF_CAN)] public static string ToStringOrNull(this object obj, string nullString = ConstsK10.NULL_STRING)
    {
		if( obj == null ) return nullString;
		if (obj is IEnumerable enumerable)
		{
			if( enumerable is string str ) return str;
			var count = -1;
			var countStr = "...";
			if (obj is ICollection collection) count = collection.Count;
			if( count >= 0 ) countStr = count.ToString();
			if( count == 0 ) return $"{obj.TypeNameOrNull()}[0]{{}}";
			var sb = StringBuilderPool.RequestWith($"{obj.TypeNameOrNull()}[{countStr}]{{ ");
			bool first = true;
			foreach (var e in enumerable)
			{
				if( !first ) sb.Append(", ");
				first = false;
				sb.Append(e.ToStringOrNull());
			}
			sb.Append( " }" );
			return sb.ReturnToPoolAndCast();
		}
        return obj.ToString();
    }
	[MethodImpl(Optimizations.INLINE_IF_CAN)] public static string WithoutColorTags( this string str ) => _colorTagRegex.Replace(str, string.Empty);

    [MethodImpl(Optimizations.INLINE_IF_CAN)] public static string ElementsToString(this IEnumerable enumerable, string nullString = ConstsK10.NULL_STRING, string separator = ", ")
    {
		if( enumerable == null ) return nullString;
		if( enumerable is string str ) return str;
		var sb = StringBuilderPool.RequestWith($"{{ ");
		bool first = true;
		foreach (var e in enumerable)
		{
			if( !first ) sb.Append( separator );
			first = false;
			if( e == null ) sb.Append( nullString );
			else if( e is IEnumerable innerEnumerable ) sb.Append( innerEnumerable.ElementsToString() );
			else sb.Append( e.ToString() );
		}
		sb.Append( " }" );
		return sb.ReturnToPoolAndCast();
    }

    [MethodImpl( Optimizations.INLINE_IF_CAN )] public static string FillSides( this string text, int totalLength, char filler = ' ' )
	{
		var padding = totalLength - text.Length;
		if( padding <= 0 ) return text.Substring( 0, totalLength );
		var left = padding / 2;
		var right = padding - left;
		return new string( filler, left ) + text + new string( filler, right );
	}

    [MethodImpl( Optimizations.INLINE_IF_CAN )] public static string ToStringOrNullColored( this object obj, Color valueColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(Colors.Console.Negation);
	[MethodImpl( Optimizations.INLINE_IF_CAN )] public static string ToStringOrNullColored( this object obj, Color valueColor, Color nullColor, string nullString = ConstsK10.NULL_STRING ) => obj != null ? obj.ToString().Colorfy(valueColor) : nullString.Colorfy(nullColor);
}