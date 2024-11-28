using UnityEngine;

namespace TransformOperation
{
    public class SetLocalScale : ITransformSet
    {
        [SerializeField] Vector3 localScale;

        public void Set( Transform transform )
        {
            transform.localScale = localScale;
        }
    }
}