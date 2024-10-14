using UnityEngine;

namespace TransformOperation
{
    public class SetLocalEulerRotation : ITransformSet
    {
        [SerializeField] Vector3 localEulerRotation;

        public void Set( Transform transform )
        {
            transform.localRotation = Quaternion.Euler( localEulerRotation );
        }
    }
}