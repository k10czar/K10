using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestCustomSerializedMessages : EditorWindow
{
	private const float FIELD_WIDTH = 150;
	private static readonly GUILayoutOption FIELD_WIDTH_PROP = GUILayout.Width( FIELD_WIDTH );
	private static readonly GUILayoutOption DOUBLE_FIELD_WIDTH_PROP = GUILayout.Width( 2 * FIELD_WIDTH + 5 );
	private static readonly GUILayoutOption BITMASK_WIDTH_PROP = GUILayout.Width( 70 );


	System.Type[] _messageTypes;
	string[] _displayName;
	private int _selectedMessage;
	private int _selectedConstructor;
	private readonly List<string> _objectStrings = new List<string>();
	private readonly List<object> _objects = new List<object>();

	string _serializeMethodName = "WriteBytes";
	BindingFlags _serializeMethodFlags = BindingFlags.Public | BindingFlags.Static;
	string _deserializeMethodName = "ReadBytes";
	BindingFlags _deserializeMethodFlags = BindingFlags.Public | BindingFlags.Static;
	object _instance;

	[MenuItem( "K10/Test Serialized Messages" )]
	private static void Init()
	{
		GetWindow<TestCustomSerializedMessages>( "Test RPC Custom Messages" );
	}

	public void OnEnable()
	{
		_messageTypes = GetAllSubTypesInScripts();
		_displayName = new string[_messageTypes.Length];
		for( int i = 0; i < _displayName.Length; i++ )
		{
			_displayName[i] = _messageTypes[i].ToStringOrNull();
		}
	}

	public static System.Type[] GetAllSubTypesInScripts()
	{
		var result = new System.Collections.Generic.List<System.Type>();
		System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach( var A in AS )
		{
			if( !A.FullName.StartsWith( "Assembly-" ) ) continue;
			System.Type[] types = A.GetTypes();
			foreach( var T in types ) if( T.IsDefined( typeof( CustomSerializedMessage ), true ) ) result.Add( T );
		}
		return result.ToArray();
	}

	private void OnGUI()
	{
		GUILayout.Label( "Test RPC Custom Messages", K10GuiStyles.bigBoldCenterStyle );
		K10.EditorGUIExtention.SeparationLine.Horizontal();
		GUILayout.Label( $"Custom RPC Message ({_displayName.Length})", K10GuiStyles.basicCenterStyle );
		_selectedMessage = EditorGUILayout.Popup( _selectedMessage, _displayName );
		var message = _messageTypes[_selectedMessage];
		K10.EditorGUIExtention.SeparationLine.Horizontal();

		var serializeMethod = FindMethod( "serializeMethod", message, ref _serializeMethodName, ref _serializeMethodFlags );
		var deserializeMethod = FindMethod( "deserializeMethod", message, ref _deserializeMethodName, ref _deserializeMethodFlags );

		K10.EditorGUIExtention.SeparationLine.Horizontal();
		var constructors = message.GetConstructors();
		GUILayout.Label( $"{constructors.Length} Constructor(s)", K10GuiStyles.basicCenterStyle );

		var names = new List<string> { "None" };
		names.AddRange( constructors.ToList().ConvertAll<string>( ( c ) => DebugConstructor( c ) ) );
		_selectedConstructor = EditorGUILayout.Popup( _selectedConstructor + 1, names.ToArray() );
		_selectedConstructor--;

		bool hasConstructor = _selectedConstructor >= 0 && _selectedConstructor < constructors.Length;
		if( hasConstructor )
		{
			var contructor = constructors[_selectedConstructor];

			var parameters = contructor.GetParameters();
			while( _objects.Count > parameters.Length ) _objects.RemoveAt( _objects.Count - 1 );
			while( _objectStrings.Count > parameters.Length ) _objectStrings.RemoveAt( _objectStrings.Count - 1 );

			bool isDirty = false;

			for( int i = 0; i < parameters.Length; i++ )
			{
				var p = parameters[i];
				if( _objects.Count <= i )
				{
					_objects.Add( GetDefault( p.ParameterType ) );
					isDirty = true;
				}
				if( _objects[i] == null || _objects[i].GetType() != p.ParameterType )
				{
					var defVal = GetDefault( p.ParameterType );
					if( _objects[i] != defVal )
					{
						_objects[i] = defVal;
						isDirty = true;
					}
				}
				var obj = Field( _objects[i], p.ParameterType, p.ToStringOrNull(), true );
				if( obj != _objects[i] )
				{
					_objects[i] = obj;
					isDirty = true;
				}
			}
			K10.EditorGUIExtention.SeparationLine.Horizontal();
			if( isDirty ) _instance = contructor.Invoke( _objects.ToArray() );
		}
		else
		{
			if( _instance == null || _instance.GetType() != message ) _instance = GetDefault( message );
		}

		DrawInstanceInspector( "Constructed Instance", _instance, message, !hasConstructor );

		K10.EditorGUIExtention.SeparationLine.Horizontal();
		var serializationReturn = TempBytes.Get( 0 );
		var failToSerialize = false;

		try { serializationReturn = serializeMethod.Invoke( _instance, new object[] { _instance } ) as byte[]; }
		catch( System.Exception ) { failToSerialize = true; }

		var bytes = serializationReturn.Length;
		GUILayout.Label( $"Serialization {bytes} Byte(s)", K10GuiStyles.basicCenterStyle );
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( failToSerialize ) GUILayout.Label( "ERROR", BITMASK_WIDTH_PROP );
		for( int i = 0; i < bytes; i++ )
		{
			GUILayout.Label( serializationReturn.DebugBitMask( i << 3, 8 ), BITMASK_WIDTH_PROP );
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		if( !failToSerialize )
		{
			var deserailizationReturn = deserializeMethod.Invoke( _instance, new object[] { serializationReturn } );
			K10.EditorGUIExtention.SeparationLine.Horizontal();
			DrawInstanceInspector( "Deserializated Instance", deserailizationReturn, message );
		}

		K10.EditorGUIExtention.SeparationLine.Horizontal();
	}

	static object Field( object obj, System.Type type, string name, bool canEdit = false )
	{
		EditorGUILayout.BeginHorizontal();
		if( canEdit )
		{
			var converter = System.ComponentModel.TypeDescriptor.GetConverter( type );
			var retStr = GUILayout.TextField( obj.ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
			try { obj = converter.ConvertFrom( retStr ); }
			catch { obj = GetDefault( type ); }
			EditorGUI.BeginDisabledGroup( true );
			GUILayout.TextField( obj.ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
		}
		else 
		{
			EditorGUI.BeginDisabledGroup( true );
			GUILayout.TextField( obj.ToStringOrNull(), DOUBLE_FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
		}
		GUILayout.Label( name );
		EditorGUILayout.EndHorizontal();
		return obj;
	}

	private static void DrawInstanceInspector( string name, System.Object instance, System.Type type, bool canEdit = false )
	{
		try { GUILayout.Label( $"{name}: {instance.ToStringOrNull()}", K10GuiStyles.basicCenterStyle ); }
		catch( System.Exception ex ) { GUILayout.Label( $"{name}: ToString ERROR({ex.GetType()})", K10GuiStyles.basicCenterStyle ); }

		var fields = type.GetFields();
		if( fields.Length > 0 ) GUILayout.Label( "fields", K10GuiStyles.smallStyle );
		for( int i = 0; i < fields.Length; i++ )
		{
			var field = fields[i];
			object obj = "ERROR";
			try { obj = field.GetValue( instance ); } catch( System.Exception ex ) { obj = $"ERROR({ex.GetType()})"; }
			var ret = Field( obj, field.FieldType, field.ToStringOrNull(), canEdit );
			if( canEdit && obj != ret ) field.SetValue( instance, ret );
		}

		var properties = type.GetProperties();
		if( properties.Length > 0 ) GUILayout.Label( "properties", K10GuiStyles.smallStyle );
		for( int i = 0; i < properties.Length; i++ )
		{
			var prop = properties[i];
			object obj = "ERROR";
			try { obj = prop.GetValue( instance ); } catch( System.Exception ex ) { obj = $"ERROR({ex.GetType()})"; }
			Field( obj, prop.PropertyType, prop.ToStringOrNull(), false );
		}
	}

	private static object GetDefault( System.Type type )
	{
		if( type.IsValueType ) return System.Activator.CreateInstance( type );
		return null;
	}

	private static string DebugConstructor( ConstructorInfo cons ) => cons != null ? $"new({DebugParameters( cons.GetParameters() )})" : "Null";
	private static string DebugMethod( MethodInfo method ) => method != null ? $"{method.ReturnType} {method}({DebugParameters( method.GetParameters() )})" : "Null";

	private static string DebugParameters( ParameterInfo[] parameters )
	{
		var pStr = " ";
		for( int i = 0; i < parameters.Length; i++ )
		{
			var p = parameters[i];
			pStr += p.ToStringOrNull();
			if( i + 1 < parameters.Length ) pStr += ", ";
			else pStr += " ";
		}
		return pStr;
	}

	private static MethodInfo FindMethod( string methodDisplayName, System.Type message, ref string name, ref BindingFlags flags )
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label( $"{methodDisplayName}:" );
		name = GUILayout.TextField( name );
		flags = (BindingFlags)EditorGUILayout.EnumFlagsField( flags );
		var method = message.GetMethod( name, flags );
		GUILayout.Label( DebugMethod( method ) );
		EditorGUILayout.EndHorizontal();
		return method;
	}
}
