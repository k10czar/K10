using DG.Tweening;
using UnityEngine;

public class DoTweenScaleFromValue : ITriggerable<float>
{
    [SerializeField] Transform transform;
    [SerializeField] DoTweenBlend blend;
    [SerializeField,Unit("Seconds")] float delay = 0;
    [SerializeField] Vector3 constant = new Vector3( 0, 1, 1 );
    [SerializeField] Vector3 scalar  = new Vector3( 1, 0, 0 );

    public void Trigger( float value )
    {
        // Debug.Log( $"{transform.HierarchyNameOrNull()}.DoTweenScaleFromValue {value}" );
        if (transform == null) return;
        DoTweenBlend.GetEaseAndDuration( blend, out var duration, out var ease );
        if( blend != null ) value = blend.EaseFunction( value, 1, 0, 0 );
        var tween = transform.DOScale( constant + ( scalar * value ), duration ).SetEase( ease );
        if( delay > Mathf.Epsilon ) tween.SetDelay( delay );
    }
}
