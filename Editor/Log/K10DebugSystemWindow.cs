using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class K10DebugSystemWindow : EditorWindow
{
    private ReorderableList debugTargetsList;
    private Vector2 scrollPos;

    private bool isGameSystemsExpanded = true;
    private bool isDebugOptionsExpanded;

	[MenuItem( "K10/Log/Filter" )] static void Open() { var i = Instance; }

	// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Init() { }

	static K10DebugSystemWindow _instance;
	public static K10DebugSystemWindow Instance
	{
		get
		{
			if( _instance == null ) _instance = GetWindow<K10DebugSystemWindow>( "LogFilter" );
			return _instance;
		}
	}



    private static void DrawGameSystemDebugEnablers( string name )
    {
        GUILayout.BeginHorizontal();

        var mainColor = BoolToColor( K10DebugSystem.CanDebug( name ) );
        GuiBackgroundColorManager.New( mainColor );
        if ( GUILayout.Button( name, K10GuiStyles.bigbuttonFlatStyle ) ) K10DebugSystem.ToggleLog( name );

        GUILayout.Space(4);
        
        var verboseColor = BoolToColor( K10DebugSystem.CanDebug( name, true ) );
        GuiBackgroundColorManager.New( verboseColor );
        if ( GUILayout.Button( "Verbose", K10GuiStyles.bigbuttonFlatStyle ) ) K10DebugSystem.ToggleLog( name, true );

        GUILayout.Space(4);
        
        var visualsColor = BoolToColor( K10DebugSystem.CanDebugVisuals( name ) );
        GuiBackgroundColorManager.New( visualsColor );
        if ( GUILayout.Button( "Visuals", K10GuiStyles.bigbuttonFlatStyle ) ) K10DebugSystem.ToggleVisualsLog( name );
        GuiColorManager.Revert(3);

        GUILayout.EndHorizontal();
    }
    
    List<IK10LogCategory> categories = new List<IK10LogCategory> {
        typeof( UniverseLogCategory ).CreateInstance() as IK10LogCategory,
        typeof( ServicesLogCategory ).CreateInstance() as IK10LogCategory,
    };

    private void DrawGameSystem()
    {
        GuiBackgroundColorManager.New( Colors.DarkSlateGray.WithHue( .66f ) );
        if( GUILayout.Button( "Game Systems", K10GuiStyles.bigbuttonFlatStyle ) ) isGameSystemsExpanded = !isGameSystemsExpanded;
        GuiColorManager.Revert();

        if( !isGameSystemsExpanded ) return;

        foreach ( var cat in categories )
        {
            GUILayout.Space(4);
            DrawGameSystemDebugEnablers( cat.Name );
        }

        Space();
    }

    private void Space()
    {
        // SkyxLayout.Space();
        GUILayout.Space(5);
    }

    protected virtual void DrawCustomTargetControl() {}

    // private void DrawDebugTargets()
    // {
    //     if (!SkyxLayout.ShouldShowBlock("Debug Targets", ref isDebugOptionsExpanded)) return;

    //     var alwaysPrintErrors = K10DebugSystem.DebugErrors();
    //     var errorsText = alwaysPrintErrors ? "Always log errors" : "Log only selected";
    //     if (SkyxLayout.PlainBGHeaderButton(errorsText, alwaysPrintErrors))
    //         K10DebugSystem.ToggleDebugErrors();

    //     var debugTarget = K10DebugSystem.DebugTargets();
    //     var color = debugTarget switch
    //     {
    //         K10DebugSystem.EDebugTargets.Disabled => SkyxStyles.danger,
    //         K10DebugSystem.EDebugTargets.All => SkyxStyles.success,
    //         _ => SkyxStyles.darkerWarning,
    //     };

    //     if (SkyxLayout.PlainBGHeaderButton($"Targets: {debugTarget}", color))
    //         K10DebugSystem.ToggleDebugTargets();

    //     if (debugTarget < K10DebugSystem.EDebugTargets.OnlySelected) return;

    //     DrawCustomTargetControl();

    //     if (!Application.isPlaying) return;

    //     Space();
    //     TryAddHierarchySelection();
    //     debugTargetsList.DoLayoutList();
    //     Space();
    // }

    private void TryAddHierarchySelection()
    {
        if (!GUILayout.Button("Add Hierarchy Selection")) return;

        foreach (var obj in Selection.objects)
        {
            var newTarget = obj as GameObject;
            if (!K10DebugSystem.selectedTargets.Contains(newTarget))
                K10DebugSystem.selectedTargets.Add(newTarget);
        }
    }

    private void DebugTargetsElementDrawer(Rect rect, int index, bool isActive, bool isFocused)
    {
        var target = (GameObject)EditorGUI.ObjectField(rect, K10DebugSystem.selectedTargets[index], typeof(GameObject), true);

        if (target != null && !target.scene.IsValid()) return;

        K10DebugSystem.selectedTargets[index] = target;
    }


    private void OnEnable()
    {
        debugTargetsList = new ReorderableList(K10DebugSystem.selectedTargets, typeof(GameObject))
        {
            draggable = false,
            displayAdd = false,
            drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Targets"); },
            drawElementCallback = DebugTargetsElementDrawer,
        };
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // DrawDebugTargets();
        DrawGameSystem();

        EditorGUILayout.EndScrollView();
    }

    static readonly Color TRUE_COLOR = Colors.LimeGreen.WithAlpha( .4f );
    static readonly Color FALSE_COLOR = Colors.Crimson.WithAlpha( .4f );

    private static Color BoolToColor(bool active) => active ? TRUE_COLOR : FALSE_COLOR;
}