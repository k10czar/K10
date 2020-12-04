using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public interface IBits : IBitsQuery, IBitsManipulator { }

public interface IBitsQuery
{
	bool this[int index] { get; }
	bool Query( int id );
}

public interface IBitsManipulator
{
	bool this[int index] { set; }
	void Set( int id, bool value );
	void SetAll( bool value );
	void Flip( int id );
	void FlipAll();
}

[System.Serializable]
public class Bits : IBits
{
	[SerializeField] List<int> _array = new List<int>();

	public bool this[int index]
	{
		get => BitsManipulator.Query( _array, index );
		set { BitsManipulator.Set( _array, index, value ); }
	}

	public bool Query( int id ) => BitsManipulator.Query( _array, id );
	public void Set( int id, bool value ) => BitsManipulator.Set( _array, id, value );
	public void Flip( int id ) => BitsManipulator.Flip( _array, id);
	public void FlipAll() => BitsManipulator.FlipAll( _array );
	public void SetAll( bool value )=> BitsManipulator.SetAll( _array, value );
	public override string ToString() => BitsManipulator.ToString( _array );
}

public static class BitsManipulator
{
	public static int QuerySector32( IReadOnlyList<int> bits, int id )
	{
		if( bits.Count == 0 ) return 0;
		var lastBit = ( bits[bits.Count - 1] & ( 1 << 31 ) ) != 0;
		var fill = lastBit ? -1 : 0;
		if( bits.Count <= id ) return fill;
		return bits[id];
	}

	public static bool Query( IReadOnlyList<int> bits, int id )
	{
		GetCoords( id, out var arrayID, out var bitID );
		if( bits.Count == 0 ) return false;
		if( bits.Count <= arrayID ) return bits[bits.Count - 1].AsMaskContainsID( 31 );
		return bits[arrayID].AsMaskContainsID( bitID );
	}

	public static bool IsAll( IReadOnlyList<int> bits, bool value )
	{
		if( bits.Count == 0 ) return !value;
		var shouldBe = value ? -1 : 0;
		for( int i = 0; i < bits.Count; i++ ) if( bits[i] != shouldBe ) return false;
		return true;
	}

	public static bool IsLastBit( IReadOnlyList<int> bits, int id )
	{
		GetCoords( id, out var arrayID, out var bitID );
		return IsLastBit( bits, arrayID, bitID );
	}

	public static bool IsLastBit( IReadOnlyList<int> bits, int arrayID, int bitID ) { return ( bitID == 31 ) && ( arrayID == ( bits.Count - 1 ) ); }

	public static void Set( IList<int> bits, int id, bool value )
	{
		GetCoords( id, out var arrayID, out var bitID );
		// Debug.Log( $"Start of Set {id}({arrayID},{bitID}) to {value} {ToString( bits as IReadOnlyList<int> )} {{ {string.Join( ", ", bits.ToList().ConvertAll( ( v ) => v.ToString() ) )} }}" );
		if( bits.Count == 0 && !value ) return;

		var lastBit = bits.Count != 0 && bits[bits.Count - 1].AsMaskContainsID( 31 );
		var fill = lastBit ? -1 : 0;

		if( bits.Count <= arrayID )
		{
			if( lastBit == value ) return;
			var newCount = arrayID + 1;
			while( bits.Count <= arrayID ) bits.Add( fill );
		}

		var isLastBit = IsLastBit( bits as IReadOnlyList<int>, arrayID, bitID );
		if( isLastBit ) bits.Add( fill );

		// Debug.Log( $"Before Set {id} to {value} {ToString( bits as IReadOnlyList<int> )} {{ {string.Join( ", ", bits.ToList().ConvertAll( ( v ) => v.ToString() ) )} }} {lastBit}" );

		if( value ) bits[arrayID] = bits[arrayID] | ( 1 << bitID );
		else bits[arrayID] = bits[arrayID] & ~( 1 << bitID );

		// Debug.Log( $"After Set {id} to {value} {ToString( bits as IReadOnlyList<int> )} {{ {string.Join( ", ", bits.ToList().ConvertAll( ( v ) => v.ToString() ) )} }} {lastBit}" );

		while( bits.Count > 1 &&
				( ( ( bits[bits.Count - 1] == -1 ) && bits[bits.Count - 2].AsMaskContainsID( 31 ) ) ||
				( ( bits[bits.Count - 1] == 0 ) && !bits[bits.Count - 2].AsMaskContainsID( 31 ) ) ) )
		{
			bits.RemoveAt( bits.Count - 1 );
		}

		if( IsAll( bits as IReadOnlyList<int>, false ) ) bits.Clear();
		// Debug.Log( $"End of Set {id} to {value} {ToString( bits as IReadOnlyList<int> )} {{ {string.Join( ", ", bits.ToList().ConvertAll( ( v ) => v.ToString() ) )} }} {lastBit}" );
	}
	public static void Flip( IList<int> bits, int id ) { Set( bits, id, !Query( bits as IReadOnlyList<int>, id ) ); }
	public static void FlipAll( IList<int> bits ) { for( int i = 0; i < bits.Count; i++ ) bits[i] = ~bits[i]; }

	public static void SetAll( IList<int> bits, bool value )
	{
		bits.Clear();
		if( value ) bits.Add( -1 );
	}

	public static void GetCoords( int id, out int arrayID, out int bitID )
	{
		arrayID = id / 32;
		bitID = id % 32;
	}


	public static int LastEntropy( IReadOnlyList<int> bits )
	{
		var lastEntropy = ( bits.Count * 32 ) - 1;
		if( lastEntropy < 0 ) return 0;
		var lastBit = Query( bits, lastEntropy );
		while( lastEntropy > 0 && ( Query( bits, --lastEntropy ) == lastBit ) ) { }
		if( Query( bits, lastEntropy ) != lastBit ) lastEntropy++;
		return lastEntropy;
	}

	private static readonly StringBuilder SB = new StringBuilder();
	public static string ToString( IReadOnlyList<int> bits )
	{
		SB.Clear();
		int lastEntropy = BitsManipulator.LastEntropy( bits );
		for( int i = 0; i <= lastEntropy; i++ ) SB.Append( Query( bits, i ) ? '1' : '0' );
		SB.Append( "..." );
		return SB.ToString();
	}
}