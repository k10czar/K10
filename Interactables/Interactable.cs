using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractableLogCategory : IK10LogCategory
{
    public string Name => "🤏Interactables";
    public Color Color => Colors.DeepPink;
}

public interface IInteractableInteraction 
{ 
    bool ConsumeEvent { get; }
}

public interface IInteractionEvent : IInteractableInteraction
{
    bool TryTrigger( string key, IInteractor interactor );
}

public interface IInteractionValue<T> : IInteractableInteraction
{
    bool TryTrigger( string key, IInteractor interactor, T value );
}

public interface IInteractorTrigger : ITriggerable<IInteractor> { }
public interface IInteractorTrigger<T> : ITriggerable<IInteractor,T> { }

namespace K10.Interactables
{
    [Flags]
    public enum EInteractionTargetingType
    {
        Look = 1,
        ColliderBounds = 2,
    }

    public class Interactable : MonoBehaviour, ILoggable<InteractableLogCategory>
    {
        [OnlyOnPlay,SerializeField,ReadOnly] BoolState isBeingTargeted = new BoolState();
        [SerializeField] EInteractionTargetingType targetingType = 0;
        [Boxed,SerializeReference,ExtendedDrawer(true)] IValidator<IInteractor> condition;
        [SerializeField] Vector3 offset;
        [SerializeField] Collider areaTriggerCollider;
        [SerializeField] Collider lookCollider;
        [SerializeField] Transform lookPositionOverride;
        [SerializeField, Range(0, 10)] float maxDistance = 1;
        [SerializeReference,ExtendedDrawer] IInteractableInteraction[] interactions;
        [SerializeReference,ExtendedDrawer] ITriggerable[] targetedReactions;
        [SerializeReference,ExtendedDrawer] ITriggerable[] untargetedReactions;
        
        List<IInteractor> currentInteractors = new List<IInteractor>();


        static List<Interactable> allActiveInteractables = new List<Interactable>();

        public Vector3 Center => transform.position + transform.rotation * offset;

        public IBoolStateObserver IsBeingTargeted => isBeingTargeted;
        
        float ScaledMaxDistance => Mathf.Max( transform.lossyScale.x, transform.lossyScale.z ) * maxDistance;

        public Vector3 LookPosition => lookPositionOverride != null ? lookPositionOverride.position : transform.position;

        public bool CanBeTargetedBy( IInteractor interactor )
        {
            if( condition == null ) return true;
            return condition.Validate( interactor );
        }

        void Start()
        {
            isBeingTargeted.Synchronize(OnIsTargetedChange);
        }

        void OnEnable()
        {
            allActiveInteractables.Add( this );
        }

        void OnDisable()
        {
            allActiveInteractables.Remove(this);
            // RemoveAllInteractors();
        }

        private void OnIsTargetedChange( bool isTargeted )
        {
            if( isTargeted ) targetedReactions.TriggerAll();
            else untargetedReactions.TriggerAll();
        }

        private void RemoveAllInteractors()
        {
            if (currentInteractors.Count == 0) return;
            currentInteractors.Clear();
            UpdateIsInteracting();
        }

        public void AddInteractor( IInteractor interactor )
        {
            currentInteractors.Add( interactor );
            UpdateIsInteracting();
        }

        public void RemoveInteractor( IInteractor interactor )
        {
            currentInteractors.Remove( interactor );
            UpdateIsInteracting();
        }

        private void UpdateIsInteracting()
        {
            RemoveNullInteractors();
            isBeingTargeted.Setter(currentInteractors.Count > 0);
        }

        private void RemoveNullInteractors()
        {
            for (int i = currentInteractors.Count - 1; i >= 0; i--)
            {
                var interactor = currentInteractors[i];
                if (interactor == null || interactor.TheInteractor == null) currentInteractors.RemoveAt(i);
            }
        }

        private IEnumerable<T> GetInteractions<T>() where T : IInteractableInteraction
        {
            foreach( var interaction in interactions )
                if( interaction is T action )
                    yield return action;
        }

        public bool TryInteractEvent( string key, IInteractor interactor )
        {
            var consumed = false;
            foreach( var interaction in GetInteractions<IInteractionEvent>() )
            {
                var triggered = interaction.TryTrigger( key, interactor );
                if( triggered && interaction.ConsumeEvent ) return true;
                consumed |= true;
            }
            return consumed;
        }

        public bool TryInteractHold( string key, IInteractor interactor, bool isHolding )
        {
            var consumed = false;
            foreach( var interaction in GetInteractions<IInteractionValue<bool>>() )
            {
                var triggered = interaction.TryTrigger( key, interactor, isHolding );
                if( triggered && interaction.ConsumeEvent ) return true;
                consumed |= true;
            }
            return consumed;
        }

        public bool TryInteractAxis( string key, IInteractor interactor, float value )
        {
            var consumed = false;
            foreach( var interaction in GetInteractions<IInteractionValue<float>>() )
            {
                var triggered = interaction.TryTrigger( key, interactor, value );
                if( triggered && interaction.ConsumeEvent ) return true;
                consumed |= true;
            }
            return consumed;
        }

        public bool TryInteractAxis2d( string key, IInteractor interactor, Vector2 value )
        {
            var consumed = false;
            foreach( var interaction in GetInteractions<IInteractionValue<Vector2>>() )
            {
                var triggered = interaction.TryTrigger( key, interactor, value );
                if( triggered && interaction.ConsumeEvent ) return true;
                consumed |= true;
            }
            return consumed;
        }

        public static Interactable GetInteractable( Vector3 origin, Vector3 dir, IInteractor interactor ) => GetInteractable(new Ray(origin, dir), interactor);
        public static Interactable GetInteractable(Ray look, IInteractor interactor) => GetInteractable(allActiveInteractables, look, interactor);
        private static Interactable GetInteractable( IEnumerable<Interactable> interactables, Ray look, IInteractor interactor )
        {
            // TimeLogging<Interactable>
            Interactable bestInteractable = default;
            float bestDistanceSqr = float.MaxValue;

            foreach( var interactable in interactables )
            {
                if( interactable == null ) continue;
                // if( !interactable.isActiveAndEnabled ) continue;
                var origin = look.origin;
                var pos = interactable.Center;
                var dx = origin.x - pos.x;
                var iMaxDis = interactable.ScaledMaxDistance;
                var negiMaxDis = -iMaxDis;
                if( dx > iMaxDis || dx < negiMaxDis ) 
                {
                    interactable.DrawDebug( Color.black );
                    continue;
                }
                var dz = origin.z - pos.z;
                if( dz > iMaxDis || dz < negiMaxDis )
                {
                    interactable.DrawDebug( Color.black );
                    continue;
                }

                var distSqr = dx * dx + dz * dz;
                if( distSqr > iMaxDis * iMaxDis ) 
                {
                    interactable.DrawDebug( Color.gray );
                    continue;
                }
                if( ( interactable.targetingType & EInteractionTargetingType.Look ) != 0 )
                {
                    var collider = interactable.lookCollider;
                    if( collider == null ) 
                    {
                        interactable.DrawDebug( Color.red );
                        continue;
                    }
                    var hitted = collider.Raycast( look, out var hit, iMaxDis );
                    if( !hitted ) 
                    {
                        interactable.DrawDebug( Color.blue );
                        continue;
                    }
                }
                if( ( interactable.targetingType & EInteractionTargetingType.ColliderBounds ) != 0 )
                {
                    var area = interactable.areaTriggerCollider;
                    if( area == null ) 
                    {
                        interactable.DrawDebug( Color.red );
                        continue;
                    }
                    var insideBounds = area.bounds.Contains( look.origin );
                    if( !insideBounds ) 
                    {
                        interactable.DrawDebug( Colors.Yellow );
                        continue;
                    }
                }
                if( !interactable.CanBeTargetedBy( interactor ) )
                {
                    interactable.DrawDebug( Colors.Orange );
                    continue;
                }
                if( bestInteractable != null )
                {
                    if( distSqr > bestDistanceSqr ) 
                    {
                        interactable.DrawDebug( Color.magenta );
                        DrawRayDebug( Color.magenta, origin, interactable );
                        continue;
                    }
                }
                bestInteractable = interactable;
                bestDistanceSqr = distSqr;
                interactable.DrawDebug( Color.cyan );
                DrawRayDebug( Color.cyan, origin, interactable );
            }

            if( bestInteractable != null ) bestInteractable.DrawDebug( Color.green );
            return bestInteractable;
        }

        static void DrawRayDebug( Color color, Vector3 origin, Interactable interactable )
        {
            if( interactable.SkipVisuals() ) return;
            var targetPos = interactable.Center;
            var nDir = targetPos - origin;
            nDir.y = 0;
            nDir.Normalize();
            Debug.DrawRay( origin, nDir * interactable.ScaledMaxDistance, Color.gray );
            Debug.DrawLine( origin, targetPos, color );
        }

        void DrawDebug( Color color )
        {
            if (this.SkipVisuals()) return;
            var center = Center;
            DebugUtils.Circle(center, ScaledMaxDistance, color);
            var look = LookPosition;
            if( center.IsCloser( look ) ) return;
            Debug.DrawLine(center, LookPosition, color);
        }

        void OnDrawGizmos()
        {
            if( Application.isPlaying ) return;
            DrawDebug( this.LogColor() );
        }
    }
}