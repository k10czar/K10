using UnityEditor;
using UnityEngine;

namespace K10.Editors
{
    /// <summary>
    /// Registers <see cref="FakeProjector"/>'s debug gizmos with the scene view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <see cref="DrawGizmo"/> rather than letting the component implement <c>OnDrawGizmosSelected</c>,
    /// because Unity does not deliver gizmo messages to disabled components. Migration turns components off as
    /// part of its normal flow — a built projection disables the projector it replaced, and the utility window
    /// exposes both enable toggles — so the projections most worth inspecting are exactly the ones whose gizmos
    /// would silently never draw. Registering here sidesteps the message system entirely.
    /// </para>
    /// <para>
    /// <see cref="GizmoType.InSelectionHierarchy"/> is included so selecting a parent shows the projections
    /// beneath it, which is how these are usually arranged in a level.
    /// </para>
    /// </remarks>
    public static class FakeProjectorGizmos
    {
        static bool _drawAll;

        /// <summary>
        /// Draws every projection in the scene, not just selected ones. Off by default — it is an audit mode
        /// for finding stray or badly placed decals, not something to leave on while dressing a level.
        /// </summary>
        public static bool DrawAll
        {
            get => _drawAll;
            set
            {
                if( _drawAll == value ) return;
                _drawAll = value;
                SceneView.RepaintAll();
            }
        }

        [DrawGizmo( GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.NonSelected )]
        static void DrawGizmos( FakeProjector projector, GizmoType gizmoType )
        {
            var selected = ( gizmoType & ( GizmoType.Selected | GizmoType.InSelectionHierarchy ) ) != 0;
            if( !selected && !DrawAll ) return;

            projector.DrawDebugGizmos( selected );
        }
    }
}
