using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestRpcCustomMessages : EditorWindow
{
	private const float FIELD_WIDTH = 150;
	private static readonly GUILayoutOption FIELD_WIDTH_PROP = GUILayout.Width( FIELD_WIDTH );
	private static readonly GUILayoutOption BITMASK_WIDTH_PROP = GUILayout.Width( 70 );


	System.Type[] _messageTypes;
	string[] _displayName;
	private int _selectedMessage;
	private int _selectedConstructor;
	private readonly List<string> _objectStrings = new List<string>();
	private readonly List<object> _objects = new List<object>();

	string _serializeMethodName = "Write";
	BindingFlags _serializeMethodFlags = BindingFlags.Public | BindingFlags.Static;
	string _deserializeMethodName = "Read";
	BindingFlags _deserializeMethodFlags = BindingFlags.Public | BindingFlags.Static;

	public void OnEnable()
	{
		_messageTypes = GetAllSubTypesInScripts();
		_displayName = new string[_messageTypes.Length];
		for( int i = 0; i < _displayName.Length; i++ )
		{
			_displayName[i] = _messageTypes[i].ToStringOrNull();
		}
	}

	[MenuItem( "Relic/Test RPC Custom Messages" )]
	private static void Init()
	{
		GetWindow<TestRpcCustomMessages>( "Test RPC Custom Messages" );
	}

	public static System.Type[] GetAllSubTypesInScripts()
	{
		var result = new System.Collections.Generic.List<System.Type>();
		System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach( var A in AS )
		{
			if( !A.FullName.StartsWith( "Assembly-" ) ) continue;
			System.Type[] types = A.GetTypes();
			foreach( var T in types ) if( T.IsDefined( typeof( RpcMessage ), true ) ) result.Add( T );
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
		_selectedConstructor = EditorGUILayout.Popup( _selectedConstructor, constructors.ToList().ConvertAll<string>( ( c ) => DebugConstructor( c ) ).ToArray() );
		var contructor = constructors[_selectedConstructor];

		var parameters = contructor.GetParameters();
		while( _objects.Count > parameters.Length ) _objects.RemoveAt( _objects.Count - 1 );
		while( _objectStrings.Count > parameters.Length ) _objectStrings.RemoveAt( _objectStrings.Count - 1 );

		for( int i = 0; i < parameters.Length; i++ )
		{
			var p = parameters[i];
			EditorGUILayout.BeginHorizontal();
			if( _objectStrings.Count <= i ) _objectStrings.Add( GetDefault( p.ParameterType ).ToString() );
			var converter = System.ComponentModel.TypeDescriptor.GetConverter( p.ParameterType );
			var color = Color.white;
			try { _objects[i] = converter.ConvertFrom( _objectStrings[i] ); }
			catch { color = Color.red; }
			GuiColorManager.New( color );
			if( _objects.Count <= i ) _objects.Add( null );
			_objectStrings[i] = GUILayout.TextField( _objectStrings[i], FIELD_WIDTH_PROP );
			EditorGUI.BeginDisabledGroup( true );
			GUILayout.TextField( _objects[i].ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
			GUILayout.Label( p.ToStringOrNull() );
			GuiColorManager.Revert();
			try { _objects[i] = converter.ConvertFrom( _objectStrings[i] ); }
			catch { _objects[i] = GetDefault( p.ParameterType ); }
			EditorGUILayout.EndHorizontal();
		}
		K10.EditorGUIExtention.SeparationLine.Horizontal();

		var instance = contructor.Invoke( _objects.ToArray() );
		DrawInstanceInspector( "Constructed Instance", instance, message );

		K10.EditorGUIExtention.SeparationLine.Horizontal();
		var serializationReturn = TempBytes.Get( 0 );
		var failToSerialize = false;

		try { serializationReturn = serializeMethod.Invoke( instance, new object[] { instance } ) as byte[]; }
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
			var deserailizationReturn = deserializeMethod.Invoke( instance, new object[] { serializationReturn } );
			K10.EditorGUIExtention.SeparationLine.Horizontal();
			DrawInstanceInspector( "Deserializated Instance", deserailizationReturn, message );
		}

		K10.EditorGUIExtention.SeparationLine.Horizontal();
	}

	private static void DrawInstanceInspector( string name, System.Object obj, System.Type type )
	{
		GUILayout.Label( $"{name}: {obj.ToStringOrNull()}", K10GuiStyles.basicCenterStyle );

		var fields = type.GetFields();
		for( int i = 0; i < fields.Length; i++ )
		{
			var field = fields[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup( true );
			GUILayout.TextField( field.GetValue( obj ).ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
			GUILayout.Label( field.ToStringOrNull() );
			EditorGUILayout.EndHorizontal();
		}

		var properties = type.GetProperties();
		for( int i = 0; i < properties.Length; i++ )
		{
			var prop = properties[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup( true );
			GUILayout.TextField( prop.GetValue( obj ).ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
			GUILayout.Label( prop.ToStringOrNull() );
			EditorGUILayout.EndHorizontal();
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
		name = GUILayout.TextField( name );
		flags = (BindingFlags)EditorGUILayout.EnumFlagsField( flags );
		var method = message.GetMethod( name, flags );
		GUILayout.Label( $"{methodDisplayName}: {DebugMethod( method )})" );
		EditorGUILayout.EndHorizontal();
		return method;
	}
}
