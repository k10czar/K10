using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace K10.Editors
{
    /// <summary>
    /// Lists every projection in the loaded scenes so a level can be audited and migrated in one place, instead
    /// of hunting <see cref="Projector"/> components through the hierarchy one at a time.
    /// </summary>
    /// <remarks>
    /// Rows come in the three states a scene mid-migration actually contains: a bare <see cref="Projector"/>
    /// still waiting to be replaced, a migrated pair where a <see cref="FakeProjector"/> mirrors the projector
    /// it superseded, and a standalone FakeProjector authored without a source. The state each row exposes is
    /// the state that decides whether the replacement is actually paying off — whether it is built, and whether
    /// its material lets it batch — and all of it is editable in place.
    /// </remarks>
    public class FakeProjectorUtilityWindow : EditorWindow
    {
        const float PING_BUTTON_WIDTH = 24f;
        const float OBJECT_COLUMN_WIDTH = 190f;
        const float KIND_COLUMN_WIDTH = 96f;
        const float SCENE_COLUMN_WIDTH = 110f;
        const float TOGGLE_COLUMN_WIDTH = 34f;
        const float MODE_COLUMN_WIDTH = 58f;
        const float RUNTIME_COLUMN_WIDTH = 62f;
        const float BUILT_COLUMN_WIDTH = 54f;
        const float BATCH_COLUMN_WIDTH = 104f;
        const float ACTION_COLUMN_WIDTH = 56f;

        /// <summary>Width of the per-row kind stripe. Narrow on purpose — it is a marker, not a background.</summary>
        const float KIND_STRIPE_WIDTH = 3f;

        static readonly Color ORANGE_COLOR = new( 1f, 0.6f, 0.1f );

        // Kind and selection used to fight over the row background, which made a selected row unreadable as
        // one kind or the other. They are separate channels now: kind owns a saturated stripe down the left
        // edge, selection owns the row background — so a row can say both things at once.
        static readonly Color UNMIGRATED_STRIPE = new( 1f, 0.55f, 0.1f, 0.9f );
        static readonly Color PAIR_STRIPE = new( 0.3f, 1f, 0.45f, 0.9f );
        static readonly Color STANDALONE_STRIPE = new( 0.3f, 0.62f, 1f, 0.9f );

        /// <summary>The object Unity considers active, which is what the inspector is showing.</summary>
        static readonly Color ACTIVE_SELECTION_TINT = new( 0.30f, 0.62f, 1f, 0.28f );

        /// <summary>Also selected, but not the one the inspector is on.</summary>
        static readonly Color SELECTION_TINT = new( 0.30f, 0.62f, 1f, 0.14f );

        /// <summary>Faint banding, so a wide row stays readable across its columns.</summary>
        static readonly Color ALTERNATE_ROW_TINT = new( 1f, 1f, 1f, 0.025f );

        enum EKind
        {
            /// <summary>A Projector with no FakeProjector referencing it — the migration backlog.</summary>
            UnmigratedProjector,

            /// <summary>A FakeProjector and the Projector it replaced.</summary>
            MigratedPair,

            /// <summary>A FakeProjector authored directly, with no Projector behind it.</summary>
            StandaloneFake,
        }

        class Entry
        {
            public FakeProjector Fake;
            public Projector Projector;
            public EKind Kind;

            public GameObject Owner => Fake != null ? Fake.gameObject : Projector != null ? Projector.gameObject : null;
            public string Name => Owner != null ? Owner.name : "<destroyed>";
            /// <summary>
            /// Owning scene, or the prefab root when a stage is open — a preview scene's name is not always
            /// set, and "which prefab" is the useful answer there anyway.
            /// </summary>
            public string SceneName
            {
                get
                {
                    if( Owner == null ) return string.Empty;

                    var sceneName = Owner.scene.name;
                    if( !string.IsNullOrEmpty( sceneName ) ) return sceneName;

                    var stage = CurrentPrefabStage();
                    return stage != null ? stage.prefabContentsRoot.name : string.Empty;
                }
            }

            /// <summary>Alive means both halves the row was built from still exist — scans go stale on undo or delete.</summary>
            public bool Alive => Kind == EKind.UnmigratedProjector
                ? Projector != null
                : Fake != null && ( Kind == EKind.StandaloneFake || Projector != null );
        }

        List<Entry> _entries = new();
        Vector2 _scrollPos;

        Material _migrationMaterial;
        bool _migrateAsRuntime;

        bool _showUnmigrated = true;
        bool _showPairs = true;
        bool _showStandalone = true;

        [MenuItem( "K10/Editors/Fake Projector Utility Window" )]
        public static void ShowWindow()
        {
            var window = GetWindow( typeof( FakeProjectorUtilityWindow ), false, "Fake Projectors" );
            window.minSize = new Vector2( 900, 160 );
        }

        void OnEnable()
        {
            // Opening or closing a scene changes the whole population of the list, and nothing else would
            // make the window rescan. A prefab stage replaces it entirely, so it has to rescan on both edges.
            EditorSceneManager.sceneOpened += Handle_SceneOpened;
            EditorSceneManager.sceneClosed += Handle_SceneClosed;
            PrefabStage.prefabStageOpened += Handle_PrefabStageChanged;
            PrefabStage.prefabStageClosing += Handle_PrefabStageChanged;

            // Rows highlight what is selected, so the highlight has to follow selections made anywhere else.
            Selection.selectionChanged += Repaint;
            Rescan();
        }

        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= Handle_SceneOpened;
            EditorSceneManager.sceneClosed -= Handle_SceneClosed;
            PrefabStage.prefabStageOpened -= Handle_PrefabStageChanged;
            PrefabStage.prefabStageClosing -= Handle_PrefabStageChanged;
            Selection.selectionChanged -= Repaint;
        }

        void Handle_SceneOpened( Scene scene, OpenSceneMode mode ) => Rescan();

        void Handle_SceneClosed( Scene scene ) => Rescan();

        void Handle_PrefabStageChanged( PrefabStage stage ) => Rescan();

        /// <summary>
        /// The prefab currently open for editing, or null when the loaded scenes are what's being edited.
        /// A prefab stage takes over: while one is open its contents are what the hierarchy shows, and the
        /// scenes behind it are not what is being worked on.
        /// </summary>
        static PrefabStage CurrentPrefabStage() => PrefabStageUtility.GetCurrentPrefabStage();

        void OnGUI()
        {
            DrawContextBar();

            DrawToolbar();
            DrawMigrationSettings();
            DrawFilters();

            EditorGUILayout.Space();
            DrawHeader();

            _scrollPos = EditorGUILayout.BeginScrollView( _scrollPos );
            var index = 0;
            foreach( var entry in _entries )
            {
                if( !entry.Alive ) { Rescan(); break; }
                if( !PassesFilter( entry ) ) continue;
                DrawRow( entry, index++ );
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>Names the context being edited, so it is never ambiguous which objects the batch buttons hit.</summary>
        void DrawContextBar()
        {
            var stage = CurrentPrefabStage();
            if( stage == null )
            {
                EditorGUILayout.LabelField( "Projections in Loaded Scenes", K10GuiStyles.boldStyle );
                return;
            }

            EditorGUILayout.LabelField( $"Prefab: {stage.prefabContentsRoot.name}", K10GuiStyles.boldStyle );

            // Worth stating outright: a prefab stage has its own physics scene, so a build inside one only
            // finds receivers that are part of the prefab. A decal meant to land on level geometry has to be
            // built in the scene, where that geometry exists.
            EditorGUILayout.HelpBox(
                "Editing a prefab: builds cast against the prefab's own physics scene, so only colliders inside this prefab can receive a projection. Projections meant for level geometry should be built in the scene instead.",
                MessageType.Info );
        }

        void DrawToolbar()
        {
            var unmigrated = _entries.Count( e => e.Kind == EKind.UnmigratedProjector );
            var fakes = _entries.Count( e => e.Fake != null );

            using( new EditorGUILayout.HorizontalScope() )
            {
                if( GUILayout.Button( "Rescan", GUILayout.Width( 80 ) ) ) Rescan();

                using( new EditorGUI.DisabledScope( unmigrated == 0 ) )
                    if( GUILayout.Button( $"Migrate All ({unmigrated})", GUILayout.Width( 140 ) ) )
                        MigrateAll();

                using( new EditorGUI.DisabledScope( fakes == 0 ) )
                {
                    if( GUILayout.Button( "Build All", GUILayout.Width( 90 ) ) ) BuildAll();
                    if( GUILayout.Button( "Clear All", GUILayout.Width( 90 ) ) ) ClearAll();
                }

                using( new EditorGUI.DisabledScope( unmigrated == 0 ) )
                    if( GUILayout.Button( "Select Unmigrated", GUILayout.Width( 140 ) ) )
                        Selection.objects = _entries
                            .Where( e => e.Kind == EKind.UnmigratedProjector )
                            .Select( e => (Object)e.Owner ).ToArray();

                GUILayout.FlexibleSpace();

                // Gizmos normally follow the selection; this shows every projection at once, which is how you
                // spot the ones sitting in mid-air without clicking through the list.
                FakeProjectorGizmos.DrawAll = GUILayout.Toggle(
                    FakeProjectorGizmos.DrawAll,
                    new GUIContent( "Show All Gizmos", "Draw gizmos for every projection in the scene, not only the selected ones." ),
                    EditorStyles.miniButton, GUILayout.Width( 120 ) );
            }
        }

        void DrawMigrationSettings()
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                _migrationMaterial = (Material)EditorGUILayout.ObjectField(
                    new GUIContent( "Material Override", "Optional. Leave empty to use the shared material for each projector's render mode, which is what batches best. Migration infers the render mode from the source projector's shader." ),
                    _migrationMaterial, typeof( Material ), false, GUILayout.Width( 400 ) );

                _migrateAsRuntime = EditorGUILayout.ToggleLeft(
                    new GUIContent( "Migrate as Runtime", "Create migrated projectors in runtime mode instead of baking them at design time." ),
                    _migrateAsRuntime, GUILayout.Width( 160 ) );

                GUILayout.FlexibleSpace();
            }

            if( _migrationMaterial != null && !_migrationMaterial.enableInstancing )
                EditorGUILayout.HelpBox( $"'{_migrationMaterial.name}' does not have GPU instancing enabled, so migrated projectors will render but never batch.", MessageType.Warning );
        }

        void DrawFilters()
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                EditorGUILayout.LabelField( $"{_entries.Count} projection(s)", GUILayout.Width( 120 ) );
                _showUnmigrated = EditorGUILayout.ToggleLeft( $"Unmigrated ({_entries.Count( e => e.Kind == EKind.UnmigratedProjector )})", _showUnmigrated, GUILayout.Width( 150 ) );
                _showPairs = EditorGUILayout.ToggleLeft( $"Migrated ({_entries.Count( e => e.Kind == EKind.MigratedPair )})", _showPairs, GUILayout.Width( 150 ) );
                _showStandalone = EditorGUILayout.ToggleLeft( $"Standalone ({_entries.Count( e => e.Kind == EKind.StandaloneFake )})", _showStandalone, GUILayout.Width( 150 ) );
                GUILayout.FlexibleSpace();
            }
        }

        bool PassesFilter( Entry entry ) => entry.Kind switch
        {
            EKind.UnmigratedProjector => _showUnmigrated,
            EKind.MigratedPair => _showPairs,
            EKind.StandaloneFake => _showStandalone,
            _ => true,
        };

        void DrawHeader()
        {
            using( new EditorGUILayout.HorizontalScope() )
            {
                GUILayout.Space( PING_BUTTON_WIDTH + 4 );
                EditorGUILayout.LabelField( "Object", K10GuiStyles.boldStyle, GUILayout.Width( OBJECT_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( "Kind", K10GuiStyles.boldStyle, GUILayout.Width( KIND_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( "Scene", K10GuiStyles.boldStyle, GUILayout.Width( SCENE_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Obj", "GameObject active state." ), K10GuiStyles.boldStyle, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Proj", "Source Projector enabled state." ), K10GuiStyles.boldStyle, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Fake", "FakeProjector enabled state." ), K10GuiStyles.boldStyle, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Mode", "Orthographic or perspective projection." ), K10GuiStyles.boldStyle, GUILayout.Width( MODE_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Build", "When the projection is built: baked in the editor, or on Start at runtime." ), K10GuiStyles.boldStyle, GUILayout.Width( RUNTIME_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Baked", "Whether a projection has actually been baked yet." ), K10GuiStyles.boldStyle, GUILayout.Width( BUILT_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( new GUIContent( "Batchable", "A projection only batches when its material exists and has GPU instancing enabled." ), K10GuiStyles.boldStyle, GUILayout.Width( BATCH_COLUMN_WIDTH ) );
                EditorGUILayout.LabelField( "Actions", K10GuiStyles.boldStyle );
            }
        }

        void DrawRow( Entry entry, int index )
        {
            var rowRect = EditorGUILayout.BeginHorizontal();

            // The rect is empty during the layout pass, which DrawRect ignores.
            DrawRowBackground( rowRect, entry, index );

            if( GUILayout.Button( EditorGUIUtility.IconContent( "d_Search Icon" ), GUILayout.Width( PING_BUTTON_WIDTH ), GUILayout.Height( EditorGUIUtility.singleLineHeight ) ) )
                SelectOwner( entry );

            using( new EditorGUI.DisabledScope( true ) )
                EditorGUILayout.ObjectField( entry.Owner, typeof( GameObject ), true, GUILayout.Width( OBJECT_COLUMN_WIDTH ) );

            DrawColoredLabel( KindLabel( entry.Kind ), KindColor( entry.Kind ), KIND_COLUMN_WIDTH );
            EditorGUILayout.LabelField( entry.SceneName, K10GuiStyles.smallStyle, GUILayout.Width( SCENE_COLUMN_WIDTH ) );

            DrawActiveToggle( entry );
            DrawEnabledToggle( entry.Projector, "Toggle Projector Enabled" );
            DrawEnabledToggle( entry.Fake, "Toggle FakeProjector Enabled" );
            DrawModeButton( entry );
            DrawRuntimeButton( entry );
            DrawBakedState( entry );
            DrawBatchableState( entry );
            DrawRowActions( entry );

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Paints the row: selection state as the background, kind as a stripe down the left edge. Selection
        /// wins the background because it is the transient thing the eye needs to find, while kind is stable
        /// and reads fine from a stripe plus the coloured label.
        /// </summary>
        void DrawRowBackground( Rect rowRect, Entry entry, int index )
        {
            var owner = entry.Owner;
            var isActiveSelection = owner != null && Selection.activeGameObject == owner;
            var isSelected = isActiveSelection || ( owner != null && Selection.Contains( owner ) );

            if( isActiveSelection ) EditorGUI.DrawRect( rowRect, ACTIVE_SELECTION_TINT );
            else if( isSelected ) EditorGUI.DrawRect( rowRect, SELECTION_TINT );
            else if( index % 2 == 1 ) EditorGUI.DrawRect( rowRect, ALTERNATE_ROW_TINT );

            var stripe = new Rect( rowRect.x, rowRect.y, KIND_STRIPE_WIDTH, rowRect.height );
            EditorGUI.DrawRect( stripe, KindStripe( entry.Kind ) );
        }

        /// <summary>
        /// Selects the row's object, additively when ctrl/cmd is held so a multi-selection can be assembled
        /// from the list the same way it would be in the hierarchy.
        /// </summary>
        static void SelectOwner( Entry entry )
        {
            var owner = entry.Owner;
            if( owner == null ) return;

            var additive = Event.current != null && ( Event.current.control || Event.current.command );
            if( additive )
            {
                var selection = new List<Object>( Selection.objects );
                if( selection.Contains( owner ) ) selection.Remove( owner );
                else selection.Add( owner );
                Selection.objects = selection.ToArray();
            }
            else
            {
                Selection.activeGameObject = owner;
            }

            EditorGUIUtility.PingObject( owner );
        }

        void DrawActiveToggle( Entry entry )
        {
            var owner = entry.Owner;
            var active = owner != null && owner.activeSelf;
            var changed = EditorGUILayout.Toggle( active, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
            if( changed == active || owner == null ) return;

            Undo.RecordObject( owner, "Toggle GameObject Active" );
            owner.SetActive( changed );
            MarkDirty( owner );
        }

        /// <summary>Draws the component's enabled toggle, or a placeholder when this row has no such half.</summary>
        void DrawEnabledToggle( Behaviour behaviour, string undoName )
        {
            if( behaviour == null )
            {
                EditorGUILayout.LabelField( "—", K10GuiStyles.smallCenterStyle, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
                return;
            }

            var changed = EditorGUILayout.Toggle( behaviour.enabled, GUILayout.Width( TOGGLE_COLUMN_WIDTH ) );
            if( changed == behaviour.enabled ) return;

            Undo.RecordObject( behaviour, undoName );
            behaviour.enabled = changed;
            MarkDirty( behaviour );
        }

        /// <summary>
        /// Toggles orthographic/perspective. Edits the Projector when one is present, since a migrated
        /// FakeProjector mirrors it and would overwrite anything set on the fake at the next build.
        /// </summary>
        void DrawModeButton( Entry entry )
        {
            if( entry.Projector != null )
            {
                var ortho = entry.Projector.orthographic;
                if( GUILayout.Button( ortho ? "Ortho" : "Persp", EditorStyles.miniButton, GUILayout.Width( MODE_COLUMN_WIDTH ) ) )
                {
                    Undo.RecordObject( entry.Projector, "Toggle Projection Mode" );
                    entry.Projector.orthographic = !ortho;
                    MarkDirty( entry.Projector );
                    if( entry.Fake != null ) RebuildFake( entry.Fake );
                }
                return;
            }

            if( entry.Fake == null )
            {
                EditorGUILayout.LabelField( "—", K10GuiStyles.smallCenterStyle, GUILayout.Width( MODE_COLUMN_WIDTH ) );
                return;
            }

            if( !GUILayout.Button( entry.Fake.Orthographic ? "Ortho" : "Persp", EditorStyles.miniButton, GUILayout.Width( MODE_COLUMN_WIDTH ) ) ) return;

            SetFakeProperty( entry.Fake, "_orthographic", property => property.boolValue = !property.boolValue, "Toggle Projection Mode" );
            RebuildFake( entry.Fake );
        }

        void DrawRuntimeButton( Entry entry )
        {
            if( entry.Fake == null )
            {
                EditorGUILayout.LabelField( "—", K10GuiStyles.smallCenterStyle, GUILayout.Width( RUNTIME_COLUMN_WIDTH ) );
                return;
            }

            var runtime = entry.Fake.IsRuntime;
            var previous = GUI.backgroundColor;
            GUI.backgroundColor = runtime ? new Color( 0.45f, 0.75f, 1f ) : new Color( 0.85f, 0.75f, 0.4f );

            if( GUILayout.Button( runtime ? "Runtime" : "Editor", EditorStyles.miniButton, GUILayout.Width( RUNTIME_COLUMN_WIDTH ) ) )
                SetFakeProperty( entry.Fake, "_runtime", property => property.boolValue = !property.boolValue, "Toggle Build Mode" );

            GUI.backgroundColor = previous;
        }

        void DrawBakedState( Entry entry )
        {
            if( entry.Fake == null )
            {
                DrawColoredLabel( "—", null, BUILT_COLUMN_WIDTH );
                return;
            }

            var baked = entry.Fake.Baked;
            if( !baked.Built )
            {
                DrawColoredLabel( "no", ORANGE_COLOR, BUILT_COLUMN_WIDTH );
                return;
            }

            // Built but floating: the raycast found nothing, so the decal sits at the fallback distance rather
            // than on a surface. It renders, which is exactly why it needs calling out.
            if( !baked.HitReceiver )
            {
                DrawColoredLabel( "fallback", ORANGE_COLOR, BUILT_COLUMN_WIDTH );
                return;
            }

            DrawColoredLabel( "yes", Color.green, BUILT_COLUMN_WIDTH );
        }

        void DrawBatchableState( Entry entry )
        {
            if( entry.Fake == null )
            {
                DrawColoredLabel( "—", null, BATCH_COLUMN_WIDTH );
                return;
            }

            // Only reachable when the decal shader failed to load from Resources, since every render mode
            // otherwise resolves to a shared material.
            if( entry.Fake.Material == null )
            {
                DrawColoredLabel( "shader missing", Color.red, BATCH_COLUMN_WIDTH );
                return;
            }

            var batchable = entry.Fake.IsBatchable;
            if( !batchable )
            {
                DrawColoredLabel( "no instancing", ORANGE_COLOR, BATCH_COLUMN_WIDTH );
                return;
            }

            // Worth distinguishing: an override still batches, but only with other projectors sharing that
            // exact material, whereas the render-mode defaults pool across the whole level.
            var overridden = entry.Fake.HasMaterialOverride;
            DrawColoredLabel( overridden ? "yes (override)" : $"yes ({entry.Fake.RenderMode})", Color.green, BATCH_COLUMN_WIDTH );
        }

        void DrawRowActions( Entry entry )
        {
            if( entry.Kind == EKind.UnmigratedProjector )
            {
                if( GUILayout.Button( "Migrate", GUILayout.Width( ACTION_COLUMN_WIDTH + 12 ) ) )
                {
                    Migrate( entry.Projector );
                    Rescan();
                }
                return;
            }

            if( GUILayout.Button( "Build", GUILayout.Width( ACTION_COLUMN_WIDTH ) ) ) RebuildFake( entry.Fake );

            if( !GUILayout.Button( "Clear", GUILayout.Width( ACTION_COLUMN_WIDTH ) ) ) return;
            Undo.RecordObject( entry.Fake, "Clear Fake Projector" );
            entry.Fake.Clear();
            MarkDirty( entry.Fake );
        }

        // ─── Operations ──────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a <see cref="FakeProjector"/> to the projector's own GameObject, points it at that projector,
        /// and bakes it. The material override is optional — without one the projection uses the shared
        /// material for its render mode, which is both the batching-friendly default and what lets a whole
        /// scene be migrated with nothing configured up front.
        /// </summary>
        void Migrate( Projector projector )
        {
            if( projector == null ) return;

            var go = projector.gameObject;
            var fake = go.GetComponent<FakeProjector>() ?? Undo.AddComponent<FakeProjector>( go );

            var serialized = new SerializedObject( fake );
            serialized.FindProperty( "_refProjector" ).objectReferenceValue = projector;
            serialized.FindProperty( "_runtime" ).boolValue = _migrateAsRuntime;
            if( _migrationMaterial != null ) serialized.FindProperty( "_material" ).objectReferenceValue = _migrationMaterial;
            serialized.ApplyModifiedProperties();

            // Always builds now: with no override the shared render-mode material takes over, and the mode
            // itself is inferred from the projector being replaced.
            RebuildFake( fake );
            MarkDirty( fake );
        }

        void MigrateAll()
        {
            var pending = _entries.Where( e => e.Kind == EKind.UnmigratedProjector ).Select( e => e.Projector ).ToList();
            if( !Confirm( "Migrate Projectors", $"Add a FakeProjector to {pending.Count} projector(s) in the loaded scenes?" ) ) return;

            RunBatch( "Migrating Projectors", pending, Migrate, p => p != null ? p.gameObject.name : string.Empty );
            Rescan();
        }

        void BuildAll()
        {
            var fakes = _entries.Where( e => e.Fake != null ).Select( e => e.Fake ).ToList();
            if( !Confirm( "Build Fake Projectors", $"Rebuild {fakes.Count} fake projector(s)?" ) ) return;

            RunBatch( "Building Fake Projectors", fakes, RebuildFake, f => f != null ? f.gameObject.name : string.Empty );
            Repaint();
        }

        void ClearAll()
        {
            var fakes = _entries.Where( e => e.Fake != null ).Select( e => e.Fake ).ToList();
            if( !Confirm( "Clear Fake Projectors", $"Discard the bake of {fakes.Count} fake projector(s)? Their settings are kept." ) ) return;

            RunBatch( "Clearing Fake Projectors", fakes, fake =>
            {
                Undo.RecordObject( fake, "Clear Fake Projector" );
                fake.Clear();
                MarkDirty( fake );
            }, f => f != null ? f.gameObject.name : string.Empty );
            Repaint();
        }

        static void RebuildFake( FakeProjector fake )
        {
            if( fake == null ) return;
            Undo.RecordObject( fake, "Build Fake Projector" );
            fake.Build();
            MarkDirty( fake );
        }

        static bool Confirm( string title, string message ) => EditorUtility.DisplayDialog( title, message, "Go ahead", "Cancel" );

        /// <summary>Runs an operation over a list behind a cancellable progress bar, so a large scene stays interruptible.</summary>
        static void RunBatch<T>( string title, IReadOnlyList<T> items, System.Action<T> action, System.Func<T, string> describe )
        {
            try
            {
                for( int i = 0; i < items.Count; i++ )
                {
                    if( EditorUtility.DisplayCancelableProgressBar( title, $"{describe( items[i] )} ({i + 1}/{items.Count})", (float)i / items.Count ) )
                    {
                        Debug.LogWarning( $"[{nameof( FakeProjectorUtilityWindow )}] {title} cancelled after {i} of {items.Count}." );
                        return;
                    }

                    action( items[i] );
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>Edits a private serialized field of a FakeProjector, which has no public setters by design.</summary>
        static void SetFakeProperty( FakeProjector fake, string propertyName, System.Action<SerializedProperty> edit, string undoName )
        {
            var serialized = new SerializedObject( fake );
            var property = serialized.FindProperty( propertyName );
            if( property == null )
            {
                Debug.LogError( $"[{nameof( FakeProjectorUtilityWindow )}] {nameof( FakeProjector )} has no serialized field '{propertyName}'." );
                return;
            }

            edit( property );
            // ApplyModifiedProperties registers its own undo step, so recording one here would double it up.
            Undo.SetCurrentGroupName( undoName );
            serialized.ApplyModifiedProperties();
            MarkDirty( fake );
        }

        static void MarkDirty( Object target )
        {
            if( target == null || Application.isPlaying ) return;
            EditorUtility.SetDirty( target );

            var component = target as Component;
            var go = component != null ? component.gameObject : target as GameObject;
            if( go != null && go.scene.IsValid() ) EditorSceneManager.MarkSceneDirty( go.scene );
        }

        // ─── Scanning ────────────────────────────────────────────────────────────

        /// <summary>
        /// Rebuilds the list from the loaded scenes. Pairs are resolved from the FakeProjector side, since that
        /// is where the reference lives — anything a FakeProjector claims is a pair, and every projector left
        /// unclaimed is migration backlog.
        /// </summary>
        /// <summary>
        /// Collects the components to list from whichever context is being edited.
        /// </summary>
        /// <remarks>
        /// A prefab stage keeps its contents in a preview scene, which the global find deliberately skips —
        /// so the prefab case has to walk down from the stage root instead, or the window would show the
        /// scenes sitting behind the stage rather than the prefab actually open.
        /// </remarks>
        static (FakeProjector[] fakes, Projector[] projectors) FindSources()
        {
            var stage = CurrentPrefabStage();
            if( stage == null )
                return (FindObjectsByType<FakeProjector>( FindObjectsInactive.Include, FindObjectsSortMode.None ),
                        FindObjectsByType<Projector>( FindObjectsInactive.Include, FindObjectsSortMode.None ));

            var root = stage.prefabContentsRoot;
            return (root.GetComponentsInChildren<FakeProjector>( true ),
                    root.GetComponentsInChildren<Projector>( true ));
        }

        void Rescan()
        {
            _entries.Clear();

            var (fakes, projectors) = FindSources();
            var claimed = new HashSet<Projector>();

            foreach( var fake in fakes )
            {
                var reference = fake.RefProjector;
                if( reference != null ) claimed.Add( reference );

                _entries.Add( new Entry
                {
                    Fake = fake,
                    Projector = reference,
                    Kind = reference != null ? EKind.MigratedPair : EKind.StandaloneFake,
                } );
            }

            foreach( var projector in projectors )
            {
                if( claimed.Contains( projector ) ) continue;
                _entries.Add( new Entry { Projector = projector, Kind = EKind.UnmigratedProjector } );
            }

            // Backlog first — the list is mostly used to work it down.
            _entries = _entries
                .OrderBy( e => (int)e.Kind )
                .ThenBy( e => e.SceneName )
                .ThenBy( e => e.Name )
                .ToList();

            Repaint();
        }

        // ─── Presentation ────────────────────────────────────────────────────────

        static Color KindStripe( EKind kind ) => kind switch
        {
            EKind.UnmigratedProjector => UNMIGRATED_STRIPE,
            EKind.MigratedPair => PAIR_STRIPE,
            _ => STANDALONE_STRIPE,
        };

        static Color KindColor( EKind kind ) => kind switch
        {
            EKind.UnmigratedProjector => ORANGE_COLOR,
            EKind.MigratedPair => Color.green,
            _ => Color.cyan,
        };

        static string KindLabel( EKind kind ) => kind switch
        {
            EKind.UnmigratedProjector => "Projector",
            EKind.MigratedPair => "Migrated",
            _ => "Fake only",
        };

        static void DrawColoredLabel( string label, Color? color, float width )
        {
            var previousColor = GUI.color;
            if( color.HasValue ) GUI.color = color.Value;

            EditorGUILayout.LabelField( label, K10GuiStyles.smallStyle, GUILayout.Width( width ) );
            GUI.color = previousColor;
        }
    }
}
