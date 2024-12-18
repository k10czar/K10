using UnityEngine;

public interface IInteractor
{ 
    GameObject TheInteractor { get; }
    IReferenceHolder<IInteractionObject> Object { get; }
    IReferenceHolder<K10.Interactables.Interactable> Target { get; }

    void Hold( IInteractionObject getReference );
    void Drop();
}

public interface IInteractionObject : System.IEquatable<IInteractionObject>
{

}

namespace K10.Interactables
{
    public class InteractorBehaviour : MonoBehaviour, IInteractor, ILoggable<InteractableLogCategory>
    {
        [SerializeField] Vector3 originOffset = Vector3.up;
        [SerializeField] Transform directionGuide;

        [SerializeField] CachedReference<Interactable> target = new();
        [SerializeField] CachedReference<IInteractionObject> interactionObject = new();

        public IReferenceHolder<Interactable> Target => target;
        public IReferenceHolder<IInteractionObject> Object => interactionObject;

        public Interactable CurrentReference => target.CurrentReference;
        public IEventRegister<Interactable, IEventValidator> OnReferenceSet => target.OnReferenceSet;
        public IEventRegister<Interactable> OnReferenceRemove => target.OnReferenceRemove;
        public IEventValidator Validator => target.Validator;
        public bool IsNull => target.IsNull;

        public GameObject TheInteractor => gameObject;

        public void Update()
        {
            var rot = transform.rotation;
            var origin = transform.position + ( rot * originOffset );
            
            var dirT = directionGuide;
            if( dirT == null ) dirT = transform;

            var dir = dirT.forward;
            var calculatedTarget = Interactable.GetInteractable( origin, dir, this );
            target.ChangeReference( calculatedTarget );
            if( this.SkipVisuals() ) return;
            DebugUtils.Circle( origin, dir, dirT.up, .2f, this.LogColor(), true );
        }

        public void Hold( IInteractionObject getReference )
        {
            this.LogVerbose( $"{TheInteractor.HierarchyNameOrNullColored(Colors.Console.Names)} now holds {getReference.ToStringOrNullColored(Colors.Console.Success)}" );
            interactionObject.ChangeReference( getReference );
        }

        public void Drop()
        {
            this.LogVerbose( $"{this.HierarchyNameOrNullColored(Colors.Console.Names)} dropped {interactionObject.CurrentReference.ToStringOrNullColored(Colors.Console.LightDanger)}" );
            interactionObject.Clear();
        }

        void OnDrawGizmos()
        {
            if( this.SkipVisuals() ) return;
            var rot = transform.rotation;
            var origin = transform.position + ( rot * originOffset );

            var dirT = directionGuide;
            if( dirT == null ) dirT = transform;

            var dir = dirT.forward;
            DebugUtils.Circle( origin, dir, dirT.up, .2f, this.LogColor(), true );
        }
    }
}