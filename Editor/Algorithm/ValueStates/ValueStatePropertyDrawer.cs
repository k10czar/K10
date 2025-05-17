#if TRY_USE_NEW_MATH && !DO_NOT_USE_NEW_MATH
#define USE_NEW_MATHEMATICS
#endif
using UnityEditor;
using UnityEngine;
using CPD = UnityEditor.CustomPropertyDrawer;

#if USE_NEW_MATHEMATICS
using Unity.Mathematics;
using v3 = Unity.Mathematics.float3;
#else
using v3 = UnityEngine.Vector3;
#endif

[CPD( typeof( BoolState ) )] public class BoolStatePropertyDrawer : ValueStatePropertyDrawer<bool> { }
[CPD( typeof( ByteState ) )] public class ByteStatePropertyDrawer : ValueStatePropertyDrawer<byte> { }
[CPD( typeof( IntState ) )] public class IntStatePropertyDrawer : ValueStatePropertyDrawer<int> { }
[CPD( typeof( FloatState ) )] public class FloatStatePropertyDrawer : ValueStatePropertyDrawer<float> { }
[CPD( typeof( DoubleState ) )] public class DoubleStatePropertyDrawer : ValueStatePropertyDrawer<double> { }
[CPD( typeof( LongState ) )] public class LongStatePropertyDrawer : ValueStatePropertyDrawer<long> { }
[CPD( typeof( UIntState ) )] public class UIntStatePropertyDrawer : ValueStatePropertyDrawer<uint> { }
[CPD( typeof( Vector2State ) )] public class Vector2StatePropertyDrawer : ValueStatePropertyDrawer<Vector2> { }
[CPD( typeof( Vector3State ) )] public class Vector3StatePropertyDrawer : ValueStatePropertyDrawer<Vector3> { }
// [CPD( typeof( V3State ) )] public class V3StatePropertyDrawer : ValueStatePropertyDrawer<v3> { }

public class ValueStatePropertyDrawer<T> : PropertyDrawer
{
	public override void OnGUI( Rect area, SerializedProperty property, GUIContent label ) { property.ValueStateField<T>( area, label ); }
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) => EditorGUIUtility.singleLineHeight;
}
