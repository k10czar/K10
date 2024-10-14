using UnityEngine;

namespace TransformOperation
{
    public class SetEulerRotation : ITransformSet
    {
        [SerializeField] Vector3 eulerRotation;

        public void Set( Transform transform )
        {
            transform.rotation = Quaternion.Euler( eulerRotation );
        }
    }
}