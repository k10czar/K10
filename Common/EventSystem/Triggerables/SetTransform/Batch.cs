using UnityEngine;

namespace TransformOperation
{
    public class Batch : ITriggerable
    {
        [SerializeField] Transform transform;
        [SerializeReference,ExtendedDrawer] ITransformSet[] transformations;

        public void Trigger()
        {
            transformations.Set( transform );
        }
    }
}