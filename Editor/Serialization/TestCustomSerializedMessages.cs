using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ReflectionExtensions
{
	public static object GetDefaultValue( this System.Type type )
	{
		if( type.IsValueType ) return type.CreateInstance();
		return null;
	}
}

public class TestCustomSerializedMessages : EditorWindow
{
	private static float DEFAULT_FIELD_WIDTH => 150;
	private static readonly GUILayoutOption FIELD_WIDTH_PROP = GUILayout.Width( DEFAULT_FIELD_WIDTH );
	private static readonly GUILayoutOption DOUBLE_FIELD_WIDTH_PROP = GUILayout.Width( 2 * DEFAULT_FIELD_WIDTH + 5 );
	private static readonly GUILayoutOption BITMASK_WIDTH_PROP = GUILayout.Width( 70 );

	public class ConstructorData
	{
		int _selectedConstructor = -1;
		string _objectStr;
		string _name;
		object _instance;
		ConstructorInfo[] _constructors;
		string[] _constructorNames;
		bool _hasConstructors = false;
		System.Type _type;
		private readonly List<ConstructorData> _parameters = new List<ConstructorData>();

		public object Instance => _instance;

		public bool SetType( System.Type type, string name )
		{
			if( type == _type ) return false;
			_type = type;
			_constructors = type.GetConstructors();
			_hasConstructors = _constructors.Length > 0;
			_instance = type.GetDefaultValue();
			var names = new List<string> { "None" };
			names.AddRange( _constructors.ToList().ConvertAll<string>( ( c ) => DebugConstructor( c ) ) );
			_constructorNames = names.ToArray();
			_name = name + ": " + _type.ToStringOrNull();
			return true;
		}

		public bool Draw()
		{
			EditorGUILayout.BeginHorizontal();
			bool isDirty = false;

			if( _hasConstructors )
			{
				var initialSelection = _selectedConstructor;
				_selectedConstructor = EditorGUILayout.Popup( _selectedConstructor + 1, _constructorNames );
				_selectedConstructor--;
				isDirty |= initialSelection != _selectedConstructor;
			}

			if( _instance == null || _instance.GetType() != _type )
			{
				_instance = _type.GetDefaultValue();
				isDirty |= true;
			}

			bool hasConstructor = _hasConstructors && _selectedConstructor >= 0 && _selectedConstructor < _constructors.Length;
			if( !hasConstructor )
			{
				// var indentLevel = EditorGUI.indentLevel;
				// EditorGUI.indentLevel = 0;
				var obj = Field( _instance, _type, _name, true );
				// EditorGUI.indentLevel = indentLevel;
				if( obj != _instance )
				{
					_instance = obj;
					isDirty = true;
				}
			}
			else
			{
				GUILayout.Label( _name );
			}

			EditorGUILayout.EndHorizontal();
			if( !hasConstructor ) return isDirty;

			var contructor = _constructors[_selectedConstructor];

			var parameters = contructor.GetParameters();
			while( _parameters.Count > parameters.Length ) _parameters.RemoveAt( _parameters.Count - 1 );
			while( _parameters.Count < parameters.Length ) _parameters.Add( new ConstructorData() );

			EditorGUI.indentLevel++;
			for( int i = 0; i < parameters.Length; i++ )
			{
				var p = parameters[i];
				var cd = _parameters[i];
				isDirty |= cd.SetType( p.ParameterType, p.Name );
				isDirty |= cd.Draw();
			}
			EditorGUI.indentLevel--;
			if( isDirty ) _instance = contructor.Invoke( _parameters.ConvertAll( ( cd ) => cd.Instance ).ToArray() );

			return isDirty;
		}
	}

	System.Type[] _messageTypes;
	string[] _displayName;
	private int _selectedMessage;
	private int _selectedConstructor;
	private readonly List<string> _objectStrings = new List<string>();
	private readonly List<object> _objects = new List<object>();

	private readonly ConstructorData _data = new ConstructorData();

	string _serializeMethodName = "WriteBytes";
	BindingFlags _serializeMethodFlags = BindingFlags.Public | BindingFlags.Static;
	string _deserializeMethodName = "ReadBytes";
	BindingFlags _deserializeMethodFlags = BindingFlags.Public | BindingFlags.Static;
	object _instance;
	Vector2 _scroll;

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
		// var r = Screen.width;
		var w = Screen.width / 3;
		FIELD_WIDTH_PROP = GUILayout.Width( w );
		DOUBLE_FIELD_WIDTH_PROP = GUILayout.Width( 2 * w + 5 );

		GUILayout.Label( "Test RPC Custom Messages", K10GuiStyles.bigBoldCenterStyle );
		K10.EditorGUIExtention.SeparationLine.Horizontal();
		GUILayout.Label( $"Custom RPC Message ({_displayName.Length})", K10GuiStyles.basicCenterStyle );
		_selectedMessage = EditorGUILayout.Popup( _selectedMessage, _displayName );

		if( _selectedMessage < 0 || _selectedMessage >= _messageTypes.Length ) return;
		var message = _messageTypes[_selectedMessage];
		K10.EditorGUIExtention.SeparationLine.Horizontal();

		_scroll = GUILayout.BeginScrollView( _scroll );

		var serializeMethod = FindMethod( "serializeMethod", message, ref _serializeMethodName, ref _serializeMethodFlags );
		var deserializeMethod = FindMethod( "deserializeMethod", message, ref _deserializeMethodName, ref _deserializeMethodFlags );

		K10.EditorGUIExtention.SeparationLine.Horizontal();

		_data.SetType( message, "message" );
		_data.Draw();
		_instance = _data.Instance;

		DrawInstanceInspector( "Constructed Instance", _instance, message, true );

		K10.EditorGUIExtention.SeparationLine.Horizontal();
		var serializationReturn = ThreadSafeTempBytes.GetClean( 0 );
		var failToSerialize = false;
		var errorType = "";
		var errorStack = "";
		var errorStr = "";

		try { serializationReturn = serializeMethod.Invoke( _instance, new object[] { _instance } ) as byte[]; }
		catch( System.Exception ex ) 
		{ 
			failToSerialize = true; 
			errorType = ex.GetType().ToString(); 
			errorStr = ex.Message;
			errorStack = ex.StackTrace;
		}

		var bytes = serializationReturn.Length;
		GUILayout.Label( $"Serialization {bytes} Byte(s)", K10GuiStyles.basicCenterStyle );
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( failToSerialize )
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label( "ERROR", BITMASK_WIDTH_PROP );
			GUILayout.Label( errorType );
			GUILayout.Label( errorStr );
			GUILayout.Label( errorStack );
			EditorGUILayout.EndVertical();
		}
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

		GUILayout.EndScrollView();
	}

	static object Field( object obj, System.Type type, string name, bool canEdit = false )
	{
		EditorGUILayout.BeginHorizontal();
		if( EditorGUI.indentLevel > 0 )
		{
			GUILayout.Space( EditorGUI.indentLevel * 20 );
		}

		if( canEdit )
		{
			var converter = System.ComponentModel.TypeDescriptor.GetConverter( type );
			var retStr = GUILayout.TextField( obj.ToStringOrNull(), FIELD_WIDTH_PROP );
			EditorGUI.EndDisabledGroup();
			try { obj = converter.ConvertFrom( retStr ); }
			catch { obj = type.GetDefaultValue(); }
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
