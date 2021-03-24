using UnityEngine;
using System;

public static class SerializableGUIDExtension
{
	public static byte[] ParseAsGsGuidBytes( this string bitString )
	{
		var bytes = new byte[16];
		bytes[0] = Convert.ToByte( bitString.Substring( 6, 2 ), 16 );
		bytes[1] = Convert.ToByte( bitString.Substring( 4, 2 ), 16 );
		bytes[2] = Convert.ToByte( bitString.Substring( 2, 2 ), 16 );
		bytes[3] = Convert.ToByte( bitString.Substring( 0, 2 ), 16 );
		bytes[4] = Convert.ToByte( bitString.Substring( 10, 2 ), 16 );
		bytes[5] = Convert.ToByte( bitString.Substring( 8, 2 ), 16 );
		bytes[6] = Convert.ToByte( bitString.Substring( 14, 2 ), 16 );
		bytes[7] = Convert.ToByte( bitString.Substring( 12, 2 ), 16 );
		for( int i = 8; i < bytes.Length; i++ ) bytes[i] = Convert.ToByte( bitString.Substring( i << 1, 2 ), 16 );
		return bytes;
	}

	public static SerializableGUID ParseAsGsGuid( this string bitString ) => new SerializableGUID( bitString.ParseAsGsGuidBytes() );

	public static SerializableGUID ParseAsGsGuidOrNull( this string bitString )
	{
		if( string.IsNullOrWhiteSpace( bitString ) ) return null;
		if( bitString.Length != 32 ) return null;
		return bitString.ParseAsGsGuid();
	}
}

[Serializable]
public class SerializableGUID : IComparable<SerializableGUID>, IEquatable<SerializableGUID>
{
	[SerializeField] private byte[] _value;
	[NonSerialized] private Guid? _cachedGuid;
	public Guid Value { get { Initialize(); return _cachedGuid.Value; } }

	private void Initialize()
	{
		if( _cachedGuid != null ) return;
		if( _value == null || _value.Length != 16 ) _cachedGuid = System.Guid.Empty;
		else _cachedGuid = new System.Guid( _value );
	}
	   
	public SerializableGUID( byte[] value )
	{
		_value = value;
		_cachedGuid = new System.Guid( value );
	}

	public SerializableGUID( Guid guid )
	{
		_cachedGuid = guid;
		_value = guid.ToByteArray();
	}

	/// <summary>
	/// This method is only to be called when Updating an unsynced local item with the correct gui received from GameSparks
	/// </summary>
	/// <param name="guid"></param>
	public void SetGuid( System.Guid guid)
	{
		_cachedGuid = guid;
		_value = guid.ToByteArray();
	}

	public int CompareTo( SerializableGUID other ) => Value.CompareTo( other.Value );
	public bool Is( Guid other ) => Value.Equals( other );

	public bool Equals( SerializableGUID other ) => other != null && other.Value == this.Value;
	public bool IsValid() => _value != null && _value.Length == 16;

#if UNITY_EDITOR
	public bool Equals( byte[] byteArray )
	{
		if( byteArray.Length != _value.Length ) return false;

		for( int i = 0; i < byteArray.Length; i++ )
		{
			var element = byteArray[i];
			if( element != _value[i] ) return false;
		}

		return true;
	}

	public bool Equals( UnityEditor.SerializedProperty prop )
	{
		if( prop == null ) return false;

		if( prop.arraySize != _value.Length ) return false;

		for( int i = 0; i < prop.arraySize; i++ )
		{
			var element = prop.GetArrayElementAtIndex( i );
			if( element.intValue != _value[i] ) return false;
		}

		return true;
	}

	public void Reinitialize()
	{
		_cachedGuid = null;
		Initialize();
	}
#endif

	public override string ToString() => Value.ToString( "N" );
}
