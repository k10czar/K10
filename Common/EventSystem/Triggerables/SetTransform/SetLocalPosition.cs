using UnityEngine;

namespace TransformOperation
{
    public class SetLocalPosition : ITransformSet
    {
        [SerializeField] Vector3 localPosition;

        public void Set( Transform transform )
        {
            transform.localPosition = localPosition;
        }
    }
}