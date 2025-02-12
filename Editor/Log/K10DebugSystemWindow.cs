using System;
using System.Collections.Generic;
using K10.DebugSystem;
using K10.EditorUtils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class K10DebugSystemWindow : EditorWindow
{
    protected static readonly Color TRUE_COLOR = Colors.LimeGreen.WithAlpha( .4f );
    protected static readonly Color FALSE_COLOR = Colors.Crimson.WithAlpha( .4f );
    protected static readonly Color MORE_COLOR = TRUE_COLOR.WithHue( .1f );
    protected static readonly Color SECTION_COLOR = Colors.DarkSlateGray.WithHue( .66f );
    protected static readonly Color SECTION_HIDDEN_COLOR = Colors.DarkSlateGray.Enlight( .5f );
    protected static readonly Color MODIFIERS_COLOR = Colors.DarkSlateGray.WithHue( .1666666f );

    private ReorderableList debugTargetsList;
    private Vector2 scrollPos;

    private bool isGameSystemsExpanded = true;
    private bool isDebugOptionsExpanded;

    bool dirty = false;

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

    private void DrawGameSystemDebugEnablers(IK10LogCategory category)
    {
        var inspectorWidth = EditorGUIUtility.currentViewWidth;
        var elementWidth = (inspectorWidth - 46) / 3f;
        var width = GUILayout.Width(elementWidth);

        var categoryName = category.Name;
        var categoryType = category.GetType();

        GUILayout.BeginHorizontal();
        GUILayout.Space(4);
        GuiBackgroundColorManager.New(category.Color);

        if (GUILayout.Button(GUIContent.none, K10GuiStyles.bigbuttonFlatStyle, GUILayout.Width(20)))
        {
            var on = K10DebugSystem.CanDebug(categoryType);
            K10DebugSystem.SetCategory(categoryType, EDebugType.Default, !on);
            K10DebugSystem.SetCategory(categoryType, EDebugType.Verbose, !on);
            K10DebugSystem.SetCategory(categoryType, EDebugType.Visual, !on);
        }

        GuiBackgroundColorManager.Revert();
        GUILayout.Space(4);
        if (ToggleButton(categoryName, K10DebugSystem.CanDebug(categoryType), width)) K10DebugSystem.ToggleCategory(categoryType, EDebugType.Default);
        GUILayout.Space(4);
        if (ToggleButton("Verbose", K10DebugSystem.CanDebug(categoryType, EDebugType.Verbose), width)) K10DebugSystem.ToggleCategory(categoryType, EDebugType.Verbose);
        GUILayout.Space(4);
        if (ToggleButton("Visuals", K10DebugSystem.CanDebug(categoryType, EDebugType.Visual), width)) K10DebugSystem.ToggleCategory(categoryType, EDebugType.Visual);
        GUILayout.Space(4);
        GUILayout.EndHorizontal();
    }

    protected bool ToggleButton(string label, bool initialState, params GUILayoutOption[] options)
    {
        GuiBackgroundColorManager.New(BoolToColor(initialState));
        var changed = GUILayout.Button(label, K10GuiStyles.bigbuttonFlatStyle, options);
        GuiBackgroundColorManager.Revert();
        dirty |= changed;
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
            DrawGameSystemDebugEnablers( cat );
        }
    }

    protected static void Space() => GUILayout.Space(5);

    protected virtual void DrawCustomTargetControl() {}

    protected static bool DrawSection( string name, ref bool isExpanded, params GUILayoutOption[] options )
    {
        if( Button( name, isExpanded ? SECTION_COLOR : SECTION_HIDDEN_COLOR, K10GuiStyles.bigbuttonFlatStyle, options ) )
            isExpanded = !isExpanded;
        return !isExpanded;
    }

    private static bool Button( string name, Color color, GUIStyle style, params GUILayoutOption[] options )
    {
        GuiBackgroundColorManager.New( color );
        var changed = GUILayout.Button( name, style, options );
        GuiBackgroundColorManager.Revert();
        return changed;
    }

    private void DrawDebugTargets()
    {
        if (DrawSection("Debug Targets", ref isDebugOptionsExpanded)) return;

        Space();
        var debugTarget = K10DebugSystem.DebugTargetType();
        var color = debugTarget switch
        {
            EDebugTargets.All => TRUE_COLOR,
            _ => MORE_COLOR,
        };

        if (Button($"Targets: {debugTarget}", color, K10GuiStyles.bigbuttonFlatStyle))
            K10DebugSystem.ToggleDebugTargetType();

        if (debugTarget < EDebugTargets.ListedTarget) return;

        Space();

        debugTargetsList.DoLayoutList();

        DrawCustomTargetControl();

        if (Application.isPlaying)
            TryAddHierarchySelection();
    }

    private void TryAddHierarchySelection()
    {
        if (!GUILayout.Button("Add Hierarchy Selection")) return;

        foreach (var obj in Selection.objects)
            K10DebugSystem.ToggleDebugTarget(obj);
    }

    private static void DebugTargetsElementDrawer(Rect rect, int index, bool isActive, bool isFocused)
    {
        var text = K10DebugSystem.DebugTargets()[index];

        rect.height = 18;
        rect.y += 1;
        var result = EditorGUI.TextField(rect, text);

        if (result != text) K10DebugSystem.DebugTargets()[index] = result;
    }

    protected virtual void DrawCustomSections() {}

    private void OnEnable()
    {
        debugTargetsList = new ReorderableList(K10DebugSystem.DebugTargets(), typeof(string))
        {
            draggable = false,
            displayAdd = false,
            drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Targets"); },
            drawElementCallback = DebugTargetsElementDrawer,
            onRemoveCallback = _ => K10DebugSystem.ToggleDebugTarget(K10DebugSystem.DebugTargets()[debugTargetsList.index])
        };
    }

    private void OnGUI()
    {
        dirty = false;
        DrawConditionalCompilation();
        Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawDebugTargets();
        Space();
        DrawGameSystem();
        DrawCustomSections();

        EditorGUILayout.EndScrollView();

        if( dirty )
        {
            SceneView.RepaintAll();
            HandleUtility.Repaint();
        }
    }

    private static Color BoolToColor(bool active) => active ? TRUE_COLOR : FALSE_COLOR;


    #region Define Symbol Manipulation

    private void DrawConditionalCompilation()
    {
        var enabled = DefineSymbols.Has(K10Log.ConditionalDirective);
        var text = enabled ? "Log calls Included" : "Log calls Stripped";
        if( ToggleButton( text, enabled ) )
            DefineSymbols.Toggle(K10Log.ConditionalDirective);
    }

    #endregion
}