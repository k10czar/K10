using UnityEngine;

namespace TransformOperation
{
    public class SetPosition : ITransformSet
    {
        [SerializeField] Vector3 postion;

        public void Set( Transform transform )
        {
            transform.position = postion;
        }
    }
}