using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum EInteractionTargetingType
{
    Look = 1,
    ColliderBounds = 2,
}

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

public class Interactable : MonoBehaviour, ILogglable<InteractableLogCategory>
{
    [SerializeField,ReadOnly] BoolState isBeingTargeted = new BoolState();
    [SerializeField] EInteractionTargetingType targetingType = 0;
    [SerializeField] Vector3 offset;
    [SerializeField] Collider areaTriggerCollider;
    [SerializeField] Collider lookCollider;
    [SerializeField] Transform lookPositionOverride;
    [SerializeField, Range(0, 10)] float maxDistance = 1;
    [SerializeReference,ExtendedDrawer] IInteractableInteraction[] interactions;
    [SerializeField,ReadOnly] List<GameObject> currentInteractors = new List<GameObject>();
    [SerializeReference,ExtendedDrawer] ITriggerable[] targetedReactions;
    [SerializeReference,ExtendedDrawer] ITriggerable[] untargetedReactions;

    static List<Interactable> allActiveInteractables = new List<Interactable>();

    public Vector3 Center => transform.position + transform.rotation * offset;

    public IBoolStateObserver IsBeingTargeted => isBeingTargeted;
    
    float ScaledMaxDistance => Mathf.Max( transform.lossyScale.x, transform.lossyScale.z ) * maxDistance;

    public Vector3 LookPosition => lookPositionOverride != null ? lookPositionOverride.position : transform.position;

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

    public void AddInteractor( GameObject interactor )
    {
        currentInteractors.Add(interactor);
        UpdateIsInteracting();
    }

    public void RemoveInteractor( GameObject interactor )
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
            if (interactor == null) currentInteractors.RemoveAt(i);
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

    public static Interactable GetInteractable( Vector3 origin, Vector3 dir ) => GetInteractable(new Ray(origin, dir));
    public static Interactable GetInteractable(Ray look) => GetInteractable(allActiveInteractables, look);
    private static Interactable GetInteractable( IEnumerable<Interactable> interactables, Ray look )
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
            var dz = origin.z - pos.z;
            var distSqr = dx * dx + dz * dz;
            var iMaxDis = interactable.ScaledMaxDistance;
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
                    interactable.DrawDebug( Colors.Orange );
                    continue;
                }
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
        if( this.SkipVisuals() ) return;
        DebugUtils.Circle( Center, ScaledMaxDistance, color );
    }

    void OnDrawGizmos()
    {
        if( Application.isPlaying ) return;
        DrawDebug( this.LogColor() );
    }
}
