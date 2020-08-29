using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

[CPD( typeof( BoolState ) )] public class BoolStatePropertyDrawer : ValueStatePropertyDrawer<bool> { }
[CPD( typeof( ByteState ) )] public class ByteStatePropertyDrawer : ValueStatePropertyDrawer<byte> { }
[CPD( typeof( IntState ) )] public class IntStatePropertyDrawer : ValueStatePropertyDrawer<int> { }
[CPD( typeof( FloatState ) )] public class FloatStatePropertyDrawer : ValueStatePropertyDrawer<float> { }
[CPD( typeof( DoubleState ) )] public class DoubleStatePropertyDrawer : ValueStatePropertyDrawer<double> { }
[CPD( typeof( LongState ) )] public class LongStatePropertyDrawer : ValueStatePropertyDrawer<long> { }
[CPD( typeof( UIntState ) )] public class UIntStatePropertyDrawer : ValueStatePropertyDrawer<uint> { }
[CPD( typeof( Vector2State ) )] public class Vector2StatePropertyDrawer : ValueStatePropertyDrawer<Vector2> { }
[CPD( typeof( Vector3State ) )] public class Vector3StatePropertyDrawer : ValueStatePropertyDrawer<Vector3> { }

public class ValueStatePropertyDrawer<T> : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label ) { property.ValueStateField<T>( area, label ); }
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}
