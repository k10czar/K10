// Alternative version, with redundant code removed
using UnityEngine;
using UnityEditor;
using System.Collections;
using K10.EditorGUIExtention;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform))]
public class K10TransformInspector : Editor
{
	GUIStyle _labelStyle;

	static bool _unityDefault;

	static bool _positionAdvanced;
	static Vector3 _minPos = Vector3.zero;
	static Vector3 _maxPos = Vector3.one;
	static Vector3 _offsetPos = Vector3.zero;
	
	static bool _rotationAdvanced;
	static Vector3 _minRot = Vector3.one * -180;
	static Vector3 _maxRot = Vector3.one * 180;
	
	static bool _scaleAdvanced;
	static bool _sameRangeValue;
	static Vector3 _minScl = Vector3.one;
	static Vector3 _maxScl = Vector3.one * 2;
	static float _minBaseScl = 1;
	static float _maxBaseScl = 2;

	public override void OnInspectorGUI()
	{
		if( _labelStyle == null )
			_labelStyle = new GUIStyle( EditorStyles.label ) { alignment = TextAnchor.MiddleLeft };

		if( IconButton.Layout( "tool" + ( _unityDefault ? "on" : "off" ), 16, 'T', "Unity default Transform", Color.yellow ) ) _unityDefault = !_unityDefault;

		if( _unityDefault ) 
		{
			base.OnInspectorGUI();
			return;
		}
		
		Transform t = (Transform)target;

		Vector3 initialposition = t.localPosition;
		Vector3 initialeulerAngles = t.localEulerAngles;
		Vector3 initialscale = t.localScale;

		Vector3 position = t.localPosition;
		Vector3 eulerAngles = t.localEulerAngles;
		Vector3 scale = t.localScale;

		GUILayoutOption[] opt = new GUILayoutOption[]{ /*GUILayout.MinWidth(80.0f),*/ GUILayout.MaxHeight(20.0f) };

		if( targets.Length > 1 )
			GUI.color = Color.Lerp( Color.yellow, Color.white, .8f );

		EditorGUILayout.BeginHorizontal( opt );
		bool resetPos = IconButton.Layout( "zero", 16, '0', "Reset position to zero", Color.white );
//		bool resetPos = GUILayout.Button( "0 ", GUILayout.Width( 18f ) );
		position = EditorGUILayout.Vector3Field( "", position );
//		_positionAdvanced = GUILayout.Toggle( _positionAdvanced, "", GUILayout.Width( 20f ) );
		if( IconButton.Layout( "tool" + ( _positionAdvanced ? "on" : "off" ), 16, 'T', "Advanced position tools", Color.yellow ) ) _positionAdvanced = !_positionAdvanced;
        EditorGUILayout.EndHorizontal();

		var color = GUI.color;
		GUI.color = Color.Lerp( Color.blue, color, .9f );
		bool rndPX = false, rndPY = false, rndPZ = false, drop = false, offset = false;
        if( _positionAdvanced )
		{
			SeparationLine.Horizontal();

			EditorGUILayout.BeginHorizontal( opt );
			GUILayout.Label( "Offset", _labelStyle, GUILayout.Width( 40f ) );
			_offsetPos = EditorGUILayout.Vector3Field( "", _offsetPos );
			offset = GUILayout.Button( "Move", GUILayout.Width( 40f ) );
			EditorGUILayout.EndHorizontal();

			SeparationLine.Horizontal();

			EditorGUILayout.BeginHorizontal( opt );
			GUILayout.Label( "Randomize", _labelStyle, GUILayout.Width( 70f ) );
			rndPX = GUILayout.Button( "X?", GUILayout.Width( 25f ) );
			rndPY = GUILayout.Button( "Y?", GUILayout.Width( 25f ) );
			rndPZ = GUILayout.Button( "Z?", GUILayout.Width( 25f ) );
			var all = GUILayout.Button( "All?", GUILayout.Width( 35f ) );
			
			GUILayout.Space( 30 );
			drop = GUILayout.Button( "Drop", GUILayout.Width( 40f ) );

			if( all )
				rndPX = rndPY = rndPZ = true;

			EditorGUILayout.EndHorizontal();
//			SeparationLine.Horizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "min", GUILayout.Width( 30f ) );
			_minPos = EditorGUILayout.Vector3Field( "", _minPos );
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "max", GUILayout.Width( 30f ) );
			_maxPos = EditorGUILayout.Vector3Field( "", _maxPos );
			//			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();
			SeparationLine.Horizontal();
		}
		GUI.color = color;
		
		EditorGUILayout.BeginHorizontal( opt );
		bool resetRot = IconButton.Layout( "zero", 16, '0', "Reset rotation to zero", Color.white );
//		bool resetRot = GUILayout.Button( "0 ", GUILayout.Width( 18f ) );
		eulerAngles = EditorGUILayout.Vector3Field( "", eulerAngles );
//		_rotationAdvanced = GUILayout.Toggle( _rotationAdvanced, "", GUILayout.Width( 20f ) );
		if( IconButton.Layout( "tool" + ( _rotationAdvanced ? "on" : "off" ), 16, 'T', "Advanced rotation tools", Color.yellow ) ) _rotationAdvanced = !_rotationAdvanced;
		EditorGUILayout.EndHorizontal();

		color = GUI.color;
		GUI.color = Color.Lerp( Color.blue, color, .9f );
		bool rndRX = false, rndRY = false, rndRZ = false;
		bool faceCenter = false;
		bool awayCenter = false;
		if( _rotationAdvanced )
		{
			SeparationLine.Horizontal();
			EditorGUILayout.BeginHorizontal( opt );
			EditorGUILayout.BeginVertical();
            GUILayout.Label( "Randomize", GUILayout.Width( 70f ) );
			EditorGUILayout.EndVertical();
			rndRX = GUILayout.Button( "X?", GUILayout.Width( 25f ) );
			rndRY = GUILayout.Button( "Y?", GUILayout.Width( 25f ) );
			rndRZ = GUILayout.Button( "Z?", GUILayout.Width( 25f ) );
			var all = GUILayout.Button( "All?", GUILayout.Width( 35f ) );
			faceCenter = GUILayout.Button( "Face?", GUILayout.Width( 45f ) );
			awayCenter = GUILayout.Button( "Away?", GUILayout.Width( 50f ) );

			if( all ) 
				rndRX = rndRY = rndRZ = true;
			
			EditorGUILayout.EndHorizontal();
//			SeparationLine.Horizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "min", GUILayout.Width( 30f ) );
			_minRot = EditorGUILayout.Vector3Field( "", _minRot );
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "max", GUILayout.Width( 30f ) );
			_maxRot = EditorGUILayout.Vector3Field( "", _maxRot );
			EditorGUILayout.EndHorizontal();
			SeparationLine.Horizontal();
        }
		GUI.color = color;
		
		EditorGUILayout.BeginHorizontal( opt );
		bool resetScl = IconButton.Layout( "one", 16, '1', "Reset scale to one", Color.white );
//		bool resetScl = GUILayout.Button( "1 ", GUILayout.Width( 18f ) );
		scale = EditorGUILayout.Vector3Field( "", scale );
//		_scaleAdvanced = GUILayout.Toggle( _scaleAdvanced, "", GUILayout.Width( 20f ) );
		if( IconButton.Layout( "tool" + ( _scaleAdvanced ? "on" : "off" ), 16, 'T', "Advanced scale tools", Color.yellow ) ) _scaleAdvanced = !_scaleAdvanced;
		EditorGUILayout.EndHorizontal();
		
		color = GUI.color;
		GUI.color = Color.Lerp( Color.blue, color, .9f );
		bool rndSX = false, rndSY = false, rndSZ = false, bakeScl = false;
		if( _scaleAdvanced )
		{
			SeparationLine.Horizontal();
			EditorGUILayout.BeginHorizontal( opt );
			//GUILayout.Space( 45f );
			GUILayout.Label( "Randomize", GUILayout.Width( 70f ) );
			rndSX = GUILayout.Button( "X?", GUILayout.Width( 25f ) );
			rndSY = GUILayout.Button( "Y?", GUILayout.Width( 25f ) );
			rndSZ = GUILayout.Button( "Z?", GUILayout.Width( 25f ) );
			var all = GUILayout.Button( "All?", GUILayout.Width( 35f ) );
			if( all )
				rndSX = rndSY = rndSZ = true;

			_sameRangeValue = GUILayout.Toggle( _sameRangeValue, "Same", GUILayout.Width( 50f ) );
			bakeScl = GUILayout.Button( "Bake", GUILayout.Width( 45f ) );
			EditorGUILayout.EndHorizontal();
//			SeparationLine.Horizontal();

			if( _sameRangeValue )
			{
				var w = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 30;
				_minBaseScl = EditorGUILayout.FloatField( "min", _minBaseScl );
				_maxBaseScl = EditorGUILayout.FloatField( "max", _maxBaseScl );
				EditorGUIUtility.labelWidth = w;
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label( "min", GUILayout.Width( 30f ) );
				_minScl = EditorGUILayout.Vector3Field( "", _minScl );
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label( "max", GUILayout.Width( 30f ) );
				_maxScl = EditorGUILayout.Vector3Field( "", _maxScl );
				EditorGUILayout.EndHorizontal();
			}
			SeparationLine.Horizontal();
		}
		GUI.color = color;

		if( GUI.changed )
		{
			foreach( Transform trans in serializedObject.targetObjects )
			{
				var so = new SerializedObject( trans );
				//var current = (Transform)currentTarget;

				so.Update();
				SerializedProperty posProp = so.FindProperty( "m_LocalPosition" );
				SerializedProperty rotProp = so.FindProperty( "m_LocalRotation" );
				SerializedProperty sclProp = so.FindProperty( "m_LocalScale" );

				//Undo.RegisterUndo( current, "Transform Change" );

				var localPosition = posProp.vector3Value;
				if( !Mathf.Approximately( initialposition.x, position.x ) ) localPosition.x = position.x;
				if( !Mathf.Approximately( initialposition.y, position.y ) ) localPosition.y = position.y;
				if( !Mathf.Approximately( initialposition.z, position.z ) ) localPosition.z = position.z;

				var localEulerAngles = rotProp.quaternionValue.eulerAngles;
				if( !Mathf.Approximately( initialeulerAngles.x, eulerAngles.x ) ) localEulerAngles.x = eulerAngles.x;
				if( !Mathf.Approximately( initialeulerAngles.y, eulerAngles.y ) ) localEulerAngles.y = eulerAngles.y;
				if( !Mathf.Approximately( initialeulerAngles.z, eulerAngles.z ) ) localEulerAngles.z = eulerAngles.z;

				var localScale = sclProp.vector3Value;
				if( !Mathf.Approximately( initialscale.x, scale.x ) ) localScale.x = scale.x;
				if( !Mathf.Approximately( initialscale.y, scale.y ) ) localScale.y = scale.y;
				if( !Mathf.Approximately( initialscale.z, scale.z ) ) localScale.z = scale.z;

				if( resetPos ) localPosition = Vector3.zero;
				if( rndPX ) localPosition.x = Random.Range( _minPos.x, _maxPos.x );
				if( rndPY ) localPosition.y = Random.Range( _minPos.y, _maxPos.y );
				if( rndPZ ) localPosition.z = Random.Range( _minPos.z, _maxPos.z );


				if( offset ) localPosition += _offsetPos;

				if( resetRot ) localEulerAngles = Vector3.zero;

				if( faceCenter )
				{
					var pos = trans.position;
					localEulerAngles.y = Mathf.Atan2( pos.x, pos.z ) * Mathf.Rad2Deg - ( trans.parent != null ? trans.parent.eulerAngles.y : 0 );
				}

				if( awayCenter )
				{
					var pos = trans.position;
					localEulerAngles.y = Mathf.Atan2( pos.x, pos.z ) * Mathf.Rad2Deg - 180 - ( trans.parent != null ? trans.parent.eulerAngles.y : 0 );
				}

				if( rndRX ) localEulerAngles.x = Random.Range( _minRot.x, _maxRot.x );
				if( rndRY ) localEulerAngles.y = Random.Range( _minRot.y, _maxRot.y );
				if( rndRZ ) localEulerAngles.z = Random.Range( _minRot.z, _maxRot.z );

				var baseScale = Random.Range( _minBaseScl, _maxBaseScl );
				if( resetScl ) localScale = Vector3.one;
				if( rndSX ) localScale.x = _sameRangeValue ? baseScale : Random.Range( _minScl.x, _maxScl.x );
				if( rndSY ) localScale.y = _sameRangeValue ? baseScale : Random.Range( _minScl.y, _maxScl.y );
				if( rndSZ ) localScale.z = _sameRangeValue ? baseScale : Random.Range( _minScl.z, _maxScl.z );


				if( drop )
				{
					Vector3 pPos = ( trans.parent != null ) ? trans.parent.position : Vector3.zero;
					var pos = trans.position;
					RaycastHit hit;
					if( Physics.Raycast( pos, Vector3.down, out hit, float.MaxValue ) )
					{
						pos = hit.point - pPos;
						localPosition = pos;
					}
				}

				posProp.vector3Value = FixIfNaN( localPosition );
				rotProp.quaternionValue = Quaternion.Euler( FixIfNaN( localEulerAngles ) );
				sclProp.vector3Value = FixIfNaN( localScale );

				if( bakeScl )
					BakeScale( so );

				so.ApplyModifiedProperties();
			}
		}
	}

	private void BakeScale( SerializedObject obj )
	{
		Transform trans = obj.targetObject as Transform;
		var scl = obj.FindProperty( "m_LocalScale" );
		Vector3 oldScale = scl.vector3Value;
		scl.vector3Value = Vector3.one;

		foreach( Transform child in trans )
		{
			var soc = new SerializedObject( child );
			soc.Update();

			soc.FindProperty( "m_LocalScale" ).vector3Value = new Vector3( child.localScale.x * oldScale.x, child.localScale.y * oldScale.y, child.localScale.z * oldScale.z );
			soc.FindProperty( "m_LocalPosition" ).vector3Value = new Vector3( child.localPosition.x * oldScale.x, child.localPosition.y * oldScale.y, child.localPosition.z * oldScale.z );

			soc.ApplyModifiedProperties();
		}

		foreach( var col in trans.GetComponents<BoxCollider>() )
		{
			var soc = new SerializedObject( col );
			soc.Update();

			soc.FindProperty( "m_Size" ).vector3Value = new Vector3( col.size.x * oldScale.x, col.size.y * oldScale.y, col.size.z * oldScale.z );
			soc.FindProperty( "m_Center" ).vector3Value = new Vector3( col.center.x * oldScale.x, col.center.y * oldScale.y, col.center.z * oldScale.z );

			soc.ApplyModifiedProperties();
		}

		foreach( var col in trans.GetComponents<BoxCollider2D>() )
		{
			var soc = new SerializedObject( col );
			soc.Update();

			var it = soc.GetIterator();
			//Debug.LogWarning( "Init on " + it.name + " " + ( serializedObject.targetObject as Transform ).name );
			int count = 0;
			bool enter = it.Next( true );
			while( enter && it != null && count < 100 )
			{
				Debug.Log( it.name );
				enter = it.Next( false );
				count++;
			}

			soc.FindProperty( "m_Size" ).vector2Value = new Vector2( col.size.x * oldScale.x, col.size.y * oldScale.y );
			soc.FindProperty( "m_Offset" ).vector2Value = new Vector2( col.offset.x * oldScale.x, col.offset.y * oldScale.y );

			soc.ApplyModifiedProperties();
		}

	}
	
	private Vector3 FixIfNaN( Vector3 v )
	{
		if( float.IsNaN( v.x ) ) v.x = 0;
		if( float.IsNaN( v.y ) ) v.y = 0; 
		if( float.IsNaN( v.z ) ) v.z = 0;
		return v;
	}
	
}