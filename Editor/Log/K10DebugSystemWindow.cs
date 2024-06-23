using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class K10DebugSystemWindow : EditorWindow
{
    static readonly Color TRUE_COLOR = Colors.LimeGreen.WithAlpha( .4f );
    static readonly Color FALSE_COLOR = Colors.Crimson.WithAlpha( .4f );
    static readonly Color SECTION_COLOR = Colors.DarkSlateGray.WithHue( .66f );
    static readonly Color SECTION_HIDDEN_COLOR = Colors.DarkSlateGray.Revalue( .5f );
    static readonly Color MODIFIERS_COLOR = Colors.DarkSlateGray.WithHue( .1666666f );

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
        var inspectorWidth = EditorGUIUtility.currentViewWidth;
        var elementWidth = ( inspectorWidth - 22 ) / 3f;
        var width = GUILayout.Width( elementWidth );

        GUILayout.BeginHorizontal();
        GUILayout.Space(4);
        if( ToggleButton( name, K10DebugSystem.CanDebug( name ), width ) ) K10DebugSystem.ToggleLog( name );
        GUILayout.Space(4);
        if( ToggleButton( "Verbose", K10DebugSystem.CanDebug( name, true ), width ) ) K10DebugSystem.ToggleLog( name, true );
        GUILayout.Space(4);
        if( ToggleButton( "Visuals", K10DebugSystem.CanDebugVisuals( name ), width ) ) K10DebugSystem.ToggleVisualsLog( name );
        GUILayout.Space(4);
        GUILayout.EndHorizontal();
    }

    private static bool ToggleButton( string name, bool initialState, params GUILayoutOption[] options )
    {
        GuiBackgroundColorManager.New( BoolToColor( initialState ) );
        var changed = GUILayout.Button( name, K10GuiStyles.bigbuttonFlatStyle, options );
        GuiBackgroundColorManager.Revert();
        return changed;
    }
    
    List<IK10LogCategory> categories = null;
    IEnumerable<IK10LogCategory> Categories
    {
        get
        {
            if( categories == null )
            {
                categories = new List<IK10LogCategory>();
                foreach( var catType in TypeListDataCache.GetFrom( typeof( IK10LogCategory ) ).GetTypes() )
                {
                    if( catType == typeof(TempLogCategory) ) continue;
                    try
                    {
                        var instance = catType.CreateInstance();
                        var cat = instance as IK10LogCategory;
                        if( cat != null ) categories.Add( cat );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"{catType.ToStringOrNullColored( Colors.Console.TypeName )}: {ex.Message}" );
                    }
                }
            }
            return categories;
        }
    }

    private void DrawGameSystem()
    {
        if( DrawSection( "Game Systems", ref isGameSystemsExpanded ) ) return;

        foreach ( var cat in Categories )
        {
            Space();
            DrawGameSystemDebugEnablers( cat.Name );
        }
        
        Space();

        var inspectorWidth = EditorGUIUtility.currentViewWidth;
        var collumWidth = ( inspectorWidth - 22 ) / 3f;
        var btnWidth = ( collumWidth - 12 ) / 3f;

        GUILayout.BeginHorizontal();
        GUILayout.Space(4);
        DrawCollumModifiers( btnWidth );
        GUILayout.Space(4);
        DrawCollumModifiers( btnWidth, true );
        GUILayout.Space(4);
        DrawCollumModifiers( btnWidth, false, true );
        GUILayout.Space(4);
        GUILayout.EndHorizontal();

        Space();
        
        GUILayout.BeginVertical();
        DrawCollumModifiers( inspectorWidth - 6, true, true );
        GUILayout.EndVertical();

        Space();
    }

    private void DrawCollumModifiers( float width, bool verbose = false, bool visuals = false )
    {
        var opts = GUILayout.Width( width );

        GuiBackgroundColorManager.New( MODIFIERS_COLOR );
        if( GUILayout.Button( "All", K10GuiStyles.bigbuttonLeanFlatStyle, opts ) ) Set( true, verbose, visuals );
        GUILayout.Space(4);
        if( GUILayout.Button( "Flip", K10GuiStyles.bigbuttonLeanFlatStyle, opts ) ) Flip( verbose, visuals );
        GUILayout.Space(4);
        if( GUILayout.Button( "None", K10GuiStyles.bigbuttonLeanFlatStyle, opts ) ) Set( false, verbose, visuals );
        GuiColorManager.Revert();
    }

    private void Flip(bool verbose, bool visuals)
    {
        bool all = verbose && visuals;

        if( all )
        {
            foreach ( var cat in Categories ) 
            {
                K10DebugSystem.ToggleVisualsLog( cat.Name );
                K10DebugSystem.ToggleLog( cat.Name );
                K10DebugSystem.ToggleLog( cat.Name, verbose );
            }
        }
        else
        {
            if( visuals ) foreach ( var cat in Categories ) K10DebugSystem.ToggleVisualsLog( cat.Name );
            else foreach ( var cat in Categories ) K10DebugSystem.ToggleLog( cat.Name, verbose );
        }
    }

    private void Set( bool value, bool verbose, bool visuals)
    {
        bool all = verbose && visuals;

        if( all )
        {
            foreach ( var cat in Categories ) 
            {
                K10DebugSystem.SetVisualsLog( cat.Name, value );
                K10DebugSystem.SetLog( cat.Name, value );
                K10DebugSystem.SetLog( cat.Name, value, verbose );
            }
        }
        else
        {
            if( visuals ) foreach ( var cat in Categories ) K10DebugSystem.SetVisualsLog( cat.Name, value );
            else foreach ( var cat in Categories ) K10DebugSystem.SetLog( cat.Name, value, verbose );
        }
    }

    private void Space()
    {
        // SkyxLayout.Space();
        GUILayout.Space(5);
    }

    protected virtual void DrawCustomTargetControl() {}

    private static bool DrawSection( string name, ref bool isExpanded, params GUILayoutOption[] options )
    {
        GuiBackgroundColorManager.New( isExpanded ? SECTION_COLOR : SECTION_HIDDEN_COLOR );
        if( GUILayout.Button( name, K10GuiStyles.bigbuttonFlatStyle, options ) ) isExpanded = !isExpanded;
        GuiColorManager.Revert();
        return !isExpanded;
    }

    private void DrawDebugTargets()
    {
        if ( DrawSection("Debug Targets", ref isDebugOptionsExpanded) ) return;

        // var alwaysPrintErrors = K10DebugSystem.DebugErrors();
        // var errorsText = alwaysPrintErrors ? "Always log errors" : "Log only selected";
        // if (SkyxLayout.PlainBGHeaderButton(errorsText, alwaysPrintErrors))
        //     K10DebugSystem.ToggleDebugErrors();

        // var debugTarget = K10DebugSystem.DebugTargets();
        // var color = debugTarget switch
        // {
        //     K10DebugSystem.EDebugTargets.Disabled => SkyxStyles.danger,
        //     K10DebugSystem.EDebugTargets.All => SkyxStyles.success,
        //     _ => SkyxStyles.darkerWarning,
        // };

        // if (SkyxLayout.PlainBGHeaderButton($"Targets: {debugTarget}", color))
        //     K10DebugSystem.ToggleDebugTargets();

        // if (debugTarget < K10DebugSystem.EDebugTargets.OnlySelected) return;

        // DrawCustomTargetControl();

        // if (!Application.isPlaying) return;

        // Space();
        // TryAddHierarchySelection();
        // debugTargetsList.DoLayoutList();
    }

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

        DrawDebugTargets();
        Space();
        DrawGameSystem();

        EditorGUILayout.EndScrollView();
    }

    private static Color BoolToColor(bool active) => active ? TRUE_COLOR : FALSE_COLOR;
}