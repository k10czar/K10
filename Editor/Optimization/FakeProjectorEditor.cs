using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for <see cref="FakeProjector"/>. Two things it does beyond the default drawer: it shows the build
/// mode as a labelled state button instead of a checkbox, and it turns the source-projector field into a
/// one-click assign when a <see cref="Projector"/> is sitting on the same object or a child — which is what
/// makes converting an already-dressed scene practical.
/// </summary>
[CustomEditor( typeof( FakeProjector ) )]
[CanEditMultipleObjects]
public sealed class FakeProjectorEditor : Editor
{
    const float MODE_BUTTON_HEIGHT = 26f;

    /// <summary>Settings that come from the source projector when one is assigned, and must not be hand-edited then.</summary>
    static readonly string[] MIRRORED_PROPERTIES =
    {
        "_orthographic", "_orthographicSize", "_fieldOfView", "_aspectRatio",
        "_nearClipPlane", "_farClipPlane", "_receiverMask",
    };

    SerializedProperty _runtime;
    SerializedProperty _refProjector;
    SerializedProperty _baked;

    GUIContent _editorModeContent;
    GUIContent _runtimeModeContent;

    void OnEnable()
    {
        _runtime = serializedObject.FindProperty( "_runtime" );
        _refProjector = serializedObject.FindProperty( "_refProjector" );
        _baked = serializedObject.FindProperty( "_baked" );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawModeButton();
        EditorGUILayout.Space();
        DrawRefProjectorField();
        EditorGUILayout.Space();
        DrawRemainingProperties();
        EditorGUILayout.Space();
        DrawBuildControls();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>Draws the build mode as a button that reads its current state, rather than a bare checkbox.</summary>
    void DrawModeButton()
    {
        _editorModeContent ??= new GUIContent( " Only Editor — baked at design time", EditorGUIUtility.IconContent( "d_SceneViewFx" ).image );
        _runtimeModeContent ??= new GUIContent( " Runtime — built on Start", EditorGUIUtility.IconContent( "d_PlayButton" ).image );

        var isRuntime = _runtime.boolValue;
        var content = isRuntime ? _runtimeModeContent : _editorModeContent;

        var previous = GUI.backgroundColor;
        GUI.backgroundColor = isRuntime ? new Color( 0.45f, 0.75f, 1f ) : new Color( 0.85f, 0.75f, 0.4f );

        if( _runtime.hasMultipleDifferentValues )
            content = new GUIContent( " — mixed modes —" );

        if( GUILayout.Button( content, GUILayout.Height( MODE_BUTTON_HEIGHT ) ) )
            _runtime.boolValue = !isRuntime;

        GUI.backgroundColor = previous;
    }

    /// <summary>
    /// Three states, per the field's contract: a plain field when a reference is already set or when there is
    /// nothing to assign, and a button replacing the label when exactly one candidate is in reach.
    /// </summary>
    void DrawRefProjectorField()
    {
        var candidate = _refProjector.hasMultipleDifferentValues || _refProjector.objectReferenceValue != null
            ? null
            : FindCandidate();

        if( candidate == null )
        {
            EditorGUILayout.PropertyField( _refProjector, new GUIContent( "Ref Projector" ) );
            return;
        }

        var onSelf = candidate.gameObject == ( (FakeProjector)target ).gameObject;
        var label = onSelf ? "Ref Attached Projector" : $"Ref {candidate.gameObject.name} Projector";

        var rect = EditorGUILayout.GetControlRect();
        var buttonRect = rect.RequestLeft( EditorGUIUtility.labelWidth );

        if( GUI.Button( buttonRect, label, EditorStyles.miniButton ) )
            _refProjector.objectReferenceValue = candidate;

        EditorGUI.PropertyField( rect.CutLeft( EditorGUIUtility.labelWidth ), _refProjector, GUIContent.none );
    }

    /// <summary>The projector this component would replace: one on the same object, else the first in a child.</summary>
    Projector FindCandidate()
    {
        if( targets.Length != 1 ) return null;
        var projector = (FakeProjector)target;
        var own = projector.GetComponent<Projector>();
        return own != null ? own : projector.GetComponentInChildren<Projector>( true );
    }

    /// <summary>
    /// Draws everything the two custom fields above didn't, locking the mirrored settings while a source
    /// projector supplies them so the inspector can't disagree with what the next build will use.
    /// </summary>
    void DrawRemainingProperties()
    {
        var mirrored = !_refProjector.hasMultipleDifferentValues && _refProjector.objectReferenceValue != null;
        if( mirrored )
            EditorGUILayout.HelpBox( "Projection settings are mirrored from the referenced Projector.", MessageType.Info );

        var iterator = serializedObject.GetIterator();
        var enterChildren = true;
        while( iterator.NextVisible( enterChildren ) )
        {
            enterChildren = false;
            var name = iterator.name;
            if( name == "m_Script" || name == "_runtime" || name == "_refProjector" || name == "_baked" ) continue;

            using( new EditorGUI.DisabledScope( mirrored && System.Array.IndexOf( MIRRORED_PROPERTIES, name ) >= 0 ) )
                EditorGUILayout.PropertyField( iterator, true );
        }
    }

    void DrawBuildControls()
    {
        using( new EditorGUI.DisabledScope( _baked.hasMultipleDifferentValues ) )
            EditorGUILayout.LabelField( "Baked", DescribeBake(), EditorStyles.miniLabel );

        using( new EditorGUILayout.HorizontalScope() )
        {
            if( GUILayout.Button( "Build" ) ) ForEachTarget( "Build Fake Projector", p => p.Build() );
            if( GUILayout.Button( "Clear" ) ) ForEachTarget( "Clear Fake Projector", p => p.Clear() );
        }
    }

    string DescribeBake()
    {
        if( targets.Length != 1 ) return $"{targets.Length} selected";
        var baked = ( (FakeProjector)target ).Baked;
        return baked.Built ? $"{baked.Size.x:0.##} x {baked.Size.y:0.##} at {baked.Position}" : "not built";
    }

    void ForEachTarget( string undoName, System.Action<FakeProjector> action )
    {
        // Apply first: Build reads the serialized fields, which still hold the pre-edit values until now.
        serializedObject.ApplyModifiedProperties();

        foreach( var obj in targets )
        {
            var projector = (FakeProjector)obj;
            Undo.RecordObject( projector, undoName );
            action( projector );
            EditorUtility.SetDirty( projector );
        }

        serializedObject.Update();
    }
}
