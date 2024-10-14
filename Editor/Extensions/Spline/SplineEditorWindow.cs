using UnityEditor;
using UnityEngine;
using K10.EditorGUIExtention;
using UnityEngine.Splines;

public class SplineEditorWindow : EditorWindow
{
	Vector3 scaleMod = Vector3.one;

	static SplineEditorWindow _instance;
	public static SplineEditorWindow Instance
	{
		get
		{
			if( _instance == null ) _instance = GetWindow<SplineEditorWindow>( "Spline Extended Editor" );
			return _instance;
		}
	}

	[MenuItem( "K10/Spline/Extended Editor" )] static void Open() { var i = Instance; }

	
	private void OnGUI()
	{
		SeparationLine.Horizontal();
		EditorGUILayout.LabelField( "Spline Extended Editor", K10GuiStyles.bigBoldCenterStyle, GUILayout.Height( 28 ) );
		SeparationLine.Horizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( "Modify Scale", GUILayout.Width( 96 ) );
		scaleMod = EditorGUILayout.Vector3Field( GUIContent.none, scaleMod );
		
		var hasSpline = false;
		foreach( var t in Selection.transforms )
		{
			if( t.GetComponent<SplineContainer>() != null )
			{
				hasSpline = true;
				break;
			}
		}

		EditorGUI.BeginDisabledGroup( !hasSpline );
		if( GUILayout.Button( hasSpline ? "Modify" : "No spline selected" ) )
		{
			Undo.RecordObjects( Selection.transforms, "Scale Spline(s)" );
			foreach( var t in Selection.transforms )
			{
				var container = t.GetComponent<SplineContainer>();
				if( container == null ) continue;
				foreach( var spline in container.Splines )
				{
                    for (int i = 0; i < spline.Count; i++)
					{
                        BezierKnot cp = spline[i];
                        cp.Position = cp.Position * scaleMod;
						cp.TangentIn = cp.TangentIn * scaleMod;
						cp.TangentOut = cp.TangentOut * scaleMod;
						spline[i] = cp;
					}
				}
				EditorUtility.SetDirty( container );
			}
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
		
		foreach( var t in Selection.transforms )
		{
			var container = t.GetComponent<SplineContainer>();
			if( container == null ) continue;
			EditorGUILayout.ObjectField( container, typeof( SplineContainer ), true );
		}
	}
}
