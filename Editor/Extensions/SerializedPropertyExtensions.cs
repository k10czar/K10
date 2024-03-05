using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyExtensions
{
	private const float MAGIC_POPUP_SPACE = 15;

	private static readonly Dictionary<string, IVoidable> _events = new Dictionary<string, IVoidable>();

	public static object TriggerMethod( this SerializedProperty property, string name, params object[] parameters )
	{
		var obj = property.GetInstance( out var objType );
		var method = objType.GetMethod( name );
		return method.Invoke( obj, parameters );
	}

    public static void IterateThroughChildProps( this SerializedProperty prop, System.Action<SerializedProperty> Iteration )
    {
        string lastArray = null;

        foreach (var innProp in prop)
        {
            if (innProp is SerializedProperty sp)
            {
                if (sp.isArray)
                {
                    lastArray = sp.propertyPath + ".Array.";
                }
                else if (lastArray != null)
                {
                    var isInnerArrayProp = sp.propertyPath.StartsWith(lastArray);
                    if (!isInnerArrayProp) lastArray = null;
                    else continue;
                }
				Iteration( sp );
            }
        }
    }
	
    public static System.Type GetSerializedPropertyType(this SerializedProperty property)
    {
        object targetObject = property.serializedObject.targetObject;
		var objectType = targetObject.GetType();
        FieldInfo field = objectType.GetField(property.propertyPath, 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            return field.FieldType;
        }
        return null; // Or handle the case where the type is not found
    }
	
    public static void DrawChildProps( this SerializedProperty prop, bool includeChildren = true, float spacing = 0 )
	{
		IterateThroughChildProps( prop, ( sp ) => DrawElementLayout( sp, includeChildren, spacing ) );
	}
	
    public static void DrawChildProps( this SerializedProperty prop, Rect rect, bool includeChildren = true, float spacing = 0 )
	{
		IterateThroughChildProps( prop, ( sp ) => DrawElement( ref rect, sp, includeChildren, spacing ) );
	}
	
    public static float CalcChildPropsHeight( this SerializedProperty prop, bool includeChildren = true, float spacing = 0 )
	{
		float h = 0;
		IterateThroughChildProps( prop, ( sp ) => SumElementHeight( ref h, sp, includeChildren, spacing ) );
		return h;
	}

	private static void DrawElement( ref Rect rect, SerializedProperty sp, bool includeChildren = true, float spacing = 0 )
	{
		var type = sp.GetSerializedPropertyType();
		var drawer = PropDrawerCache.From( type );
		var h = GetCalculatedElementHeightCached( sp, includeChildren );
		var drawRect = rect.RequestTop( h );
		if( drawer != null ) drawer.OnGUI( drawRect, sp, new GUIContent( sp.displayName ) );
		else EditorGUI.PropertyField( drawRect, sp, includeChildren );
		rect = rect.CutTop( h + spacing );
	}

	private static void DrawElementLayout( SerializedProperty sp, bool includeChildren = true, float spacing = 0 )
	{
		var height = CalcChildPropsHeight( sp, includeChildren, spacing );
		Rect rect = GUILayoutUtility.GetRect( GUIContent.none, GUIStyle.none, GUILayout.Height(height) );
		DrawElement( ref rect, sp, includeChildren, spacing );
	}

	static Dictionary<string,float> _heightCache = null;
	static Dictionary<string,float> _includedChildrenHeightCache = null;
	static HashSet<string> _logged = null;
	
	private static string CompletePath( this SerializedProperty sp )
	{
		return $"{{{string.Join(",",sp.serializedObject.targetObjects.Select( o => o.GetInstanceID()))}}}.{sp.propertyPath}";
	}

	private static float GetCalculatedElementHeightCached( SerializedProperty sp, bool includeChildren = true )
    {
        var cache = GetHeightCached( includeChildren );
		var key = sp.CompletePath();
        if (cache.TryGetValue(key, out var height)) return height;
        if (_logged == null) _logged = new();
        if (!_logged.Contains(key))
        {
            _logged.Add(key);
            // Debug.LogError($"Called GetCalculatedElementHeightCached on a non-cached property: {key}");
        }
        return EditorGUIUtility.singleLineHeight;
    }

    private static Dictionary<string, float> GetHeightCached(bool includeChildren)
    {
        return includeChildren ? (_includedChildrenHeightCache ??= new()) : (_heightCache ??= new());
    }

    private static float CalculateElementHeight( SerializedProperty sp, bool includeChildren = true )
	{
		var type = sp.GetSerializedPropertyType();
		var drawer = PropDrawerCache.From( type );
		var h = ( drawer != null ) ? drawer.GetPropertyHeight( sp, new GUIContent( sp.displayName ) ) : EditorGUI.GetPropertyHeight( sp, includeChildren );
        var cache = GetHeightCached( includeChildren );
		cache[sp.CompletePath()] = h;
		return h;
	}

	private static void SumElementHeight( ref float h, SerializedProperty sp, bool includeChildren = true, float spacing = 0 )
	{
		h += CalculateElementHeight( sp, includeChildren ) + spacing;
	}

	public static float CalcSerializedReferenceHeight( this SerializedProperty prop, bool includeChildren = true, float spacing = 0 )
	{
		return EditorGUIUtility.singleLineHeight + ( ( prop.isExpanded ) ? spacing + CalcChildPropsHeight( prop, includeChildren, spacing ) : 0 );
	}

	public static void DrawSerializedReferenceLayout( this SerializedProperty prop, bool includeChildren = true, float spacing = 0 )
	{
		var type = prop.GetManagedType();
		if( type == null )
		{
			EditorGUILayout.LabelField( $"Cannot find type: {prop.managedReferenceFieldTypename}" );
			return;
		}
        EditorGUILayout.BeginHorizontal();
		var listingData = TypeListDataCache.GetFrom( type );
		var refSize = listingData.MaxWidth;
        var index = FindIndexOf( prop.managedReferenceValue, listingData );
        var newIndex = EditorGUILayout.Popup( index, listingData.GetGUIsWithIcon(), GUILayout.Width( refSize ) );
        CheckSelectionChange( prop, listingData, index, newIndex );
        EditorGUILayout.Space( MAGIC_POPUP_SPACE, false );
        var triggerSummary = prop.managedReferenceFullTypename;
        prop.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup( prop.isExpanded, triggerSummary );
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndHorizontal();
        if (!prop.isExpanded) return;
		GuiLabelWidthManager.New( refSize );
		prop.DrawChildProps( includeChildren, spacing );
		GuiLabelWidthManager.Revert();
	}
	
	public static System.Type GetManagedType( this SerializedProperty prop )
	{
		var assType = prop.managedReferenceFieldTypename;
		var splited = assType.Split( ' ' );
		if( splited.Length == 0 ) return null;
		if( splited.Length == 1 ) return TypeFinder.WithName( splited[0] );
		var typeName = splited[1];
		var assemblyName = splited[0];
		var type = TypeFinder.WithNameFromAssembly( typeName, assemblyName );
		return type;
	}

	public static void DrawSerializedReference( this SerializedProperty prop, Rect rect, bool includeChildren = true, float spacing = 0 )
    {
		var type = prop.GetManagedType();
		if( type == null )
		{
			EditorGUI.LabelField( rect, $"Cannot find type: {prop.managedReferenceFieldTypename}" );
			return;
		}
        var firstLine = rect.RequestTop(EditorGUIUtility.singleLineHeight);
        rect = rect.CutTop(EditorGUIUtility.singleLineHeight + spacing);
		var listingData = TypeListDataCache.GetFrom( type );
		var popupWidth = listingData.MaxWidth + MAGIC_POPUP_SPACE;
        var index = FindIndexOf( prop.managedReferenceValue, listingData );
        var newIndex = EditorGUI.Popup(firstLine.RequestLeft(popupWidth), index, listingData.GetGUIsWithIcon());
        CheckSelectionChange( prop, listingData, index, newIndex );
		var triggerSummarys = prop.managedReferenceFullTypename.Split( " " );
        var triggerSummary = triggerSummarys.LastOrDefault().ToStringOrNull();
        prop.isExpanded = EditorGUI.BeginFoldoutHeaderGroup( firstLine.CutLeft(popupWidth + MAGIC_POPUP_SPACE), prop.isExpanded, triggerSummary);
        EditorGUI.EndFoldoutHeaderGroup();
        if (!prop.isExpanded) return;
		GuiLabelWidthManager.New(popupWidth);
		prop.DrawChildProps(rect, includeChildren, spacing);
		GuiLabelWidthManager.Revert();
    }

	private static int FindIndexOf( object refField, TypeListData listingData )
	{
        if( refField == null ) return -1;
		var types = listingData.GetTypes();
		var triggerRefType = refField.GetType();
		for( int i = 0; i < types.Length; i++ )
		{
			if( types[i] == triggerRefType ) return i;
		}
		 return -1;
	}

    private static void CheckSelectionChange( SerializedProperty prop, TypeListData listingData, int oldIndex, int newIndex )
    {
        if( newIndex == oldIndex ) return;
		var types = listingData.GetTypes();
		var newType = (newIndex >= 0) ? types[newIndex] : null;
		var newTypeName = newType?.FullName ?? "NULL";
		var oldTypeName = ( oldIndex < 0 || oldIndex >= types.Length ) ? "MISSING" : types[oldIndex]?.FullName ?? "NULL";
		Debug.Log($"{"Changed".Colorfy( Colors.Console.Verbs )} {"SerializedReference".Colorfy( Colors.Console.TypeName )} {prop.propertyPath.Colorfy( Colors.Console.Interfaces )} type from {$"{oldTypeName}[{oldIndex}]".Colorfy(Colors.Console.TypeName)} to {$"{newTypeName}[{newIndex}]".Colorfy(Colors.Console.Numbers)}");
		prop.managedReferenceValue = newType.CreateInstance();
    }

	public static string ToFileName( this SerializedProperty prop )
	{
		return prop.serializedObject.targetObject.GetType() + "_" + PropPathParsed( prop );
	}

    public static string GetParentArrayPropPath( this SerializedProperty property )
    {
        var path = property.propertyPath;
        var pLen = path.Length;
        var it = 1;
        while( path[pLen-it] != '[' && it < path.Length ) it++;
        return path.Substring( 0, Mathf.Max( pLen - ( ".Array.data".Length + it ), 0 ) );
    }

    public static SerializedProperty GetParentArrayProp( this SerializedProperty property )
    {
        var parentPath = GetParentArrayPropPath( property );
        return property.serializedObject.FindProperty( parentPath );
    }

	public static string PropPathParsed( this SerializedProperty prop )
	{
		var path = prop.propertyPath;
		Debug.Log( path );
		path = path.Replace( ".Array.data", "" );
		// path = path.Replace("[", "");
		// path = path.Replace("]", "");
		path = path.Replace( "._", "_" );
		path = path.Replace( "._", "_" );
		path = path.Replace( ".", "_" );
		if( path.StartsWith( "_" ) ) path = path.Substring( 1, path.Length - 1 );
		return path;
	}

	public static object GetInstance( this SerializedProperty property, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) => GetInstance( property, out var t, flags );

	public static object GetInstance( this SerializedProperty property, out System.Type objType, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
	{
		var uobj = property.serializedObject.targetObject;
		object obj = uobj;
		objType = obj.GetType();
        var ppath = property.propertyPath.Replace(".Array.data[", ".[");
		var path = ppath.Split( '.' );
		for( int i = 0; i < path.Length; i++ )
		{
			var element = path[i];
			if (element.StartsWith("["))
			{
				var index = System.Convert.ToInt32(element.Substring(0).Replace("[", "").Replace("]", ""));
				var enumerable = obj as System.Collections.IEnumerable;
            	var enm = enumerable.GetEnumerator();
				for (int eId = 0; eId <= index; eId++) if( !enm.MoveNext() ) return null;
				obj =  enm.Current;
				objType = obj.GetType();
			}
			else
			{
				var field = objType.GetField( element, flags );
				try
				{
					obj = field.GetValue( obj );
				}
				catch( System.Exception ex )
				{
					Debug.LogError( $"{ex.Message} on field.GetValue( element[{i}]:{element.ToStringOrNull()} )" );
					for( int j = 0; j < path.Length; j++ ) Debug.LogError( $"element[{j}]:{path[j].ToStringOrNull()}" );
					return null;
				}
				objType = field.FieldType;
			}
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
				evnt.Void();
				_events.Remove( key );
			}
		}
	}

	public static void DebugWatcherField<T>( string key, object obj, System.Type objType, Rect rect )
	{
		var debug = _events.TryGetValue( key, out var evnt );
		var newDebug = K10.EditorGUIExtention.IconButton.Toggle.Draw( rect, debug, "DebugOn", "DebugOff" );
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
				evnt.Void();
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

	public static T Get<T>( this SerializedProperty property )
	{
		if( typeof( T ) == typeof( float ) ) return (T)(object)property.floatValue;
		if( typeof( T ) == typeof( long ) ) return (T)(object)property.longValue;
		if( typeof( T ) == typeof( uint ) ) return (T)(object)property.longValue;
		if( typeof( T ) == typeof( int ) ) return (T)(object)property.intValue;
		if( typeof( T ) == typeof( Vector2 ) ) return (T)(object)property.vector2Value;
		if( typeof( T ) == typeof( Vector3 ) ) return (T)(object)property.vector3Value;
		if( typeof( T ) == typeof( Vector2Int ) ) return (T)(object)property.vector2IntValue;
		if( typeof( T ) == typeof( Vector3Int ) ) return (T)(object)property.vector3IntValue;
		if( typeof( T ) == typeof( byte ) ) return (T)(object)property.intValue;
		if( typeof( T ) == typeof( bool ) ) return (T)(object)property.boolValue;
		if( typeof( T ) == typeof( string ) ) return (T)(object)property.stringValue;
		if( typeof( T ) == typeof( Color ) ) return (T)(object)property.colorValue;
		if( typeof( T ) == typeof( AnimationCurve ) ) return (T)(object)property.animationCurveValue;
		if( typeof( T ) == typeof( Bounds ) ) return (T)(object)property.boundsValue;
		if( typeof( T ) == typeof( BoundsInt ) ) return (T)(object)property.boundsIntValue;
		if( typeof( T ) == typeof( double ) ) return (T)(object)property.doubleValue;
		if( typeof( T ) == typeof( Vector4 ) ) return (T)(object)property.vector4Value;
		if( typeof( T ) == typeof( Quaternion ) ) return (T)(object)property.quaternionValue;
		if( typeof( T ) == typeof( Rect ) ) return (T)(object)property.rectValue;
		if( typeof( T ) == typeof( RectInt ) ) return (T)(object)property.rectIntValue;
		if( typeof( T ) == typeof( Object ) ) return (T)(object)property.objectReferenceValue;
		Debug.LogError( $"Get<T> not implemented for type {typeof( T )}" );
		return default(T);
	}

	public static bool CheckIfChanged<T>( this SerializedProperty property, T value ) => ( value == null || !property.Get<T>().Equals( value ) );

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
	public static T FieldOf<T>( this Rect area, GUIContent label, T value )
	{
		if( typeof( T ) == typeof( float ) ) return (T)(object)EditorGUI.FloatField( area, label, (float)(object)value );
		if( typeof( T ) == typeof( long ) ) return (T)(object)EditorGUI.LongField( area, label, (long)(object)value );
		if( typeof( T ) == typeof( uint ) ) return (T)(object)(uint)EditorGUI.LongField( area, label, (uint)(object)value );
		if( typeof( T ) == typeof( int ) ) return (T)(object)EditorGUI.IntField( area, label, (int)(object)value );
		if( typeof( T ) == typeof( Vector2 ) ) return (T)(object)EditorGUI.Vector2Field( area, label, (Vector2)(object)value );
		if( typeof( T ) == typeof( Vector3 ) ) return (T)(object)EditorGUI.Vector3Field( area, label, (Vector3)(object)value );
		if( typeof( T ) == typeof( Vector2Int ) ) return (T)(object)EditorGUI.Vector2IntField( area, label, (Vector2Int)(object)value );
		if( typeof( T ) == typeof( Vector3Int ) ) return (T)(object)EditorGUI.Vector3IntField( area, label, (Vector3Int)(object)value );
		if( typeof( T ) == typeof( byte ) ) return (T)(object)(byte)EditorGUI.IntField( area, label, (byte)(object)value );
		if( typeof( T ) == typeof( bool ) ) return (T)(object)EditorGUI.ToggleLeft( area, label, (bool)(object)value );
		if( typeof( T ) == typeof( string ) ) return (T)(object)EditorGUI.TextField( area, label, (string)(object)value );
		if( typeof( T ) == typeof( Color ) ) return (T)(object)EditorGUI.ColorField( area, label, (Color)(object)value );
		if( typeof( T ) == typeof( AnimationCurve ) ) return (T)(object)EditorGUI.CurveField( area, label, (AnimationCurve)(object)value );
		if( typeof( T ) == typeof( Bounds ) ) return (T)(object)EditorGUI.BoundsField( area, label, (Bounds)(object)value );
		if( typeof( T ) == typeof( BoundsInt ) ) return (T)(object)EditorGUI.BoundsIntField( area, label, (BoundsInt)(object)value );
		if( typeof( T ) == typeof( double ) ) return (T)(object)EditorGUI.DoubleField( area, label, (double)(object)value );
		if( typeof( T ) == typeof( Vector4 ) ) return (T)(object)EditorGUI.Vector4Field( area, label, (Vector4)(object)value );
		if( typeof( T ) == typeof( Quaternion ) ) return (T)(object)Quaternion.Euler( (Vector3)(object)EditorGUI.Vector3Field( area, label, ( (Quaternion)(object)value ).eulerAngles ) );
		if( typeof( T ) == typeof( Rect ) ) return (T)(object)EditorGUI.RectField( area, label, (Rect)(object)value );
		if( typeof( T ) == typeof( RectInt ) ) return (T)(object)EditorGUI.RectIntField( area, label, (RectInt)(object)value );
		if( typeof( T ) == typeof( Object ) ) return (T)(object)EditorGUI.ObjectField( area, label, (Object)(object)value, typeof( T ), true );
		Debug.LogError( $"Field not implemented for type {typeof( T )}" );
		return default( T );
	}
}