using System.Collections.Generic;
using System.Reflection;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyExtensions
{
	private static readonly Dictionary<string, IVoidable> _events = new Dictionary<string, IVoidable>();

	public static void TriggerMethod( this SerializedProperty property, string name, params object[] parameters )
	{
		var obj = property.GetInstance( out var objType );
		var method = objType.GetMethod( name );
		method.Invoke( obj, parameters );
	}

	public static object GetInstance( this SerializedProperty property, out System.Type objType, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
	{
		var uobj = property.serializedObject.targetObject;
		object obj = uobj;
		objType = obj.GetType();
		var path = property.propertyPath.Split( '.' );
		for( int i = 0; i < path.Length; i++ )
		{
			var field = objType.GetField( path[i], flags );
			obj = field.GetValue( obj );
			objType = field.FieldType;
		}

		return obj;
	}

	public static string GetKey( this SerializedProperty property ) => $"({property.serializedObject.targetObject.NameOrNull()}).{property.propertyPath}";

	public static void DebugWatcherField<T>( this SerializedProperty property, Rect debugRect )
	{
		var key = property.GetKey();
		var debug = _events.TryGetValue( key, out var evnt );
		var newDebug = IconButton.Toggle.Draw( debugRect, debug, "DebugOn", "DebugOff" );
		if( newDebug != debug )
		{
			if( newDebug )
			{
				var debugEvnt = new Voidable<T>( ( value ) => Debug.Log( $"{value} new value on {key}" ) );
				_events.Add( key, debugEvnt );
				var obj = property.GetInstance( out var objType );
				var onChangeProp = objType.GetProperty( BoolState.ON_CHANGE_PROP_NAME );
				var onChangePropValue = onChangeProp.GetValue( obj ) as IEventRegister<T>;
				onChangePropValue.Register( debugEvnt );
			}
			else
			{
				evnt.Expire();
				_events.Remove( key );
			}
		}
	}

	public static void DebugWatcherField<T>( string key, object obj, System.Type objType, Rect rect )
	{
		var debug = _events.TryGetValue( key, out var evnt );
		var newDebug = K10.EditorGUIExtention.IconButton.Toggle.Draw( rect, debug, "spy", "DebugOff" );
		if( newDebug != debug )
		{
			if( newDebug )
			{
				var debugEvnt = new Voidable<T>( ( value ) => Debug.Log( $"{value} new value on {key}" ) );
				_events.Add( key, debugEvnt );
				var onChangeProp = objType.GetProperty( BoolState.ON_CHANGE_PROP_NAME );
				var onChangePropValue = onChangeProp.GetValue( obj ) as IEventRegister<T>;
				onChangePropValue.Register( debugEvnt );
			}
			else
			{
				evnt.Expire();
				_events.Remove( key );
			}
		}
	}

	public static bool ValueStateField<T>( this SerializedProperty property, Rect area, GUIContent label )
	{
		var valueProp = property.FindPropertyRelative( "_value" );
		var indentedArea = FakeIndentManager.New( area, -24 );
		var newVal = valueProp.FieldReturnDoNotSet<T>( label, indentedArea.CutLeft( 24 ) );
		var changed = valueProp.CheckIfChanged( newVal );
		if( changed )
		{
			property.TriggerMethod( BoolState.SET_METHOD_NAME, newVal );
			valueProp.Set( newVal );
		}
		property.DebugWatcherField<T>( indentedArea.RequestLeft( 20 ) );
		FakeIndentManager.Revert();
		return changed;
	}

	public static void Set<T>( this SerializedProperty property, T value )
	{
		if( typeof( T ) == typeof( float ) ) property.floatValue = (float)(object)value;
		else if( typeof( T ) == typeof( long ) ) property.longValue = (long)(object)value;
		else if( typeof( T ) == typeof( uint ) ) property.longValue = (uint)(object)value;
		else if( typeof( T ) == typeof( int ) ) property.intValue = (int)(object)value;
		else if( typeof( T ) == typeof( Vector2 ) ) property.vector2Value = (Vector2)(object)value;
		else if( typeof( T ) == typeof( Vector3 ) ) property.vector3Value = (Vector3)(object)value;
		else if( typeof( T ) == typeof( Vector2Int ) ) property.vector2Value = (Vector2)(object)value;
		else if( typeof( T ) == typeof( Vector3Int ) ) property.vector3Value = (Vector3)(object)value;
		else if( typeof( T ) == typeof( byte ) ) property.intValue = (byte)(object)value;
		else if( typeof( T ) == typeof( bool ) ) property.boolValue = (bool)(object)value;
		else if( typeof( T ) == typeof( string ) ) property.stringValue = (string)(object)value;
		else if( typeof( T ) == typeof( Color ) ) property.colorValue = (Color)(object)value;
		else if( typeof( T ) == typeof( AnimationCurve ) ) property.animationCurveValue = (AnimationCurve)(object)value;
		else if( typeof( T ) == typeof( Bounds ) ) property.boundsValue = (Bounds)(object)value;
		else if( typeof( T ) == typeof( BoundsInt ) ) property.boundsIntValue = (BoundsInt)(object)value;
		else if( typeof( T ) == typeof( double ) ) property.doubleValue = (double)(object)value;
		else if( typeof( T ) == typeof( Vector4 ) ) property.vector4Value = (Vector4)(object)value;
		else if( typeof( T ) == typeof( Quaternion ) ) property.quaternionValue = (Quaternion)(object)value;
		else if( typeof( T ) == typeof( Rect ) ) property.rectValue = (Rect)(object)value;
		else if( typeof( T ) == typeof( RectInt ) ) property.rectIntValue = (RectInt)(object)value;
		else if( typeof( T ) == typeof( Object ) ) property.objectReferenceValue = (Object)(object)value;
		else Debug.LogError( $"Set not implemented for type {typeof(T)} value {value}" );
	}

	public static bool CheckIfChanged<T>( this SerializedProperty property, T value )
	{
		if( typeof( T ) == typeof( float ) ) return value == null || !value.Equals( property.floatValue );
		if( typeof( T ) == typeof( long ) ) return value == null || !value.Equals( property.longValue );
		if( typeof( T ) == typeof( uint ) ) return value == null || !value.Equals( property.longValue );
		if( typeof( T ) == typeof( int ) ) return value == null || !value.Equals( property.intValue );
		if( typeof( T ) == typeof( Vector2 ) ) return value == null || !value.Equals( property.vector2Value );
		if( typeof( T ) == typeof( Vector3 ) ) return value == null || !value.Equals( property.vector3Value );
		if( typeof( T ) == typeof( Vector2Int ) ) return value == null || !value.Equals( property.vector2IntValue );
		if( typeof( T ) == typeof( Vector3Int ) ) return value == null || !value.Equals( property.vector3IntValue );
		if( typeof( T ) == typeof( byte ) ) return value == null || !value.Equals( property.intValue );
		if( typeof( T ) == typeof( bool ) ) return value == null || !value.Equals( property.boolValue );
		if( typeof( T ) == typeof( string ) ) return value == null || !value.Equals( property.stringValue );
		if( typeof( T ) == typeof( Color ) ) return value == null || !value.Equals( property.colorValue );
		if( typeof( T ) == typeof( AnimationCurve ) ) return value == null || !value.Equals( property.animationCurveValue );
		if( typeof( T ) == typeof( Bounds ) ) return value == null || !value.Equals( property.boundsValue );
		if( typeof( T ) == typeof( BoundsInt ) ) return value == null || !value.Equals( property.boundsIntValue );
		if( typeof( T ) == typeof( double ) ) return value == null || !value.Equals( property.doubleValue );
		if( typeof( T ) == typeof( Vector4 ) ) return value == null || !value.Equals( property.vector4Value );
		if( typeof( T ) == typeof( Quaternion ) ) return value == null || !value.Equals( property.quaternionValue );
		if( typeof( T ) == typeof( Rect ) ) return value == null || !value.Equals( property.rectValue );
		if( typeof( T ) == typeof( RectInt ) ) return value == null || !value.Equals( property.rectIntValue );
		if( typeof( T ) == typeof( Object ) ) return value == null || !value.Equals( property.objectReferenceValue );
		Debug.LogError( $"CheckIfChanged not implemented for type {typeof( T )}" );
		return false;
	}

	public static T FieldReturnDoNotSet<T>( this SerializedProperty property, GUIContent label, Rect area )
	{
		if( typeof( T ) == typeof( float ) ) return (T)(object)EditorGUI.FloatField( area, label, property.floatValue );
		if( typeof( T ) == typeof( long ) ) return (T)(object)EditorGUI.LongField( area, label, property.longValue );
		if( typeof( T ) == typeof( uint ) ) return (T)(object)(uint)EditorGUI.LongField( area, label, property.longValue );
		if( typeof( T ) == typeof( int ) ) return (T)(object)EditorGUI.IntField( area, label, property.intValue );
		if( typeof( T ) == typeof( Vector2 ) ) return (T)(object)EditorGUI.Vector2Field( area, label, property.vector2Value );
		if( typeof( T ) == typeof( Vector3 ) ) return (T)(object)EditorGUI.Vector3Field( area, label, property.vector3Value );
		if( typeof( T ) == typeof( Vector2Int ) ) return (T)(object)EditorGUI.Vector2IntField( area, label, property.vector2IntValue );
		if( typeof( T ) == typeof( Vector3Int ) ) return (T)(object)EditorGUI.Vector3IntField( area, label, property.vector3IntValue );
		if( typeof( T ) == typeof( byte ) ) return (T)(object)(byte)EditorGUI.IntField( area, label, property.intValue );
		if( typeof( T ) == typeof( bool ) ) return (T)(object)EditorGUI.ToggleLeft( area, label, property.boolValue );
		if( typeof( T ) == typeof( string ) ) return (T)(object)EditorGUI.TextField( area, label, property.stringValue );
		if( typeof( T ) == typeof( Color ) ) return (T)(object)EditorGUI.ColorField( area, label, property.colorValue );
		if( typeof( T ) == typeof( AnimationCurve ) ) return (T)(object)EditorGUI.CurveField( area, label, property.animationCurveValue );
		if( typeof( T ) == typeof( Bounds ) ) return (T)(object)EditorGUI.BoundsField( area, label, property.boundsValue );
		if( typeof( T ) == typeof( BoundsInt ) ) return (T)(object)EditorGUI.BoundsIntField( area, label, property.boundsIntValue );
		if( typeof( T ) == typeof( double ) ) return (T)(object)EditorGUI.DoubleField( area, label, property.doubleValue );
		if( typeof( T ) == typeof( Vector4 ) ) return (T)(object)EditorGUI.Vector4Field( area, label, property.vector4Value );
		if( typeof( T ) == typeof( Quaternion ) ) return (T)(object)Quaternion.Euler( (Vector3)(object)EditorGUI.Vector3Field( area, label, ( property.quaternionValue ).eulerAngles ) );
		if( typeof( T ) == typeof( Rect ) ) return (T)(object)EditorGUI.RectField( area, label, property.rectValue );
		if( typeof( T ) == typeof( RectInt ) ) return (T)(object)EditorGUI.RectIntField( area, label, property.rectIntValue );
		if( typeof( T ) == typeof( Object ) ) return (T)(object)EditorGUI.ObjectField( area, label, property.objectReferenceValue, typeof( T ), true );
		Debug.LogError( $"Field not implemented for type {typeof( T )}" );
		return default( T );
	}
}