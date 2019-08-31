using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor( typeof( MeshFilter ) )]
public class MeshFilterInspecto : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshFilter mf = (MeshFilter)target;

		if( mf != null && mf.sharedMesh != null )
		{
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Label( "MeshCount: " + mf.sharedMesh.vertexCount );
	        EditorGUILayout.EndHorizontal();
		}
    }
}
