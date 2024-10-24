using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

[CreateAssetMenu( fileName = "DoTweenBlend", menuName = "DOTween/DoTweenBlend", order = 0 )]
public class DoTweenBlend : ScriptableObject
{
    [SerializeField] float duration = .5f;
    [SerializeField] Ease ease = Ease.InOutCubic;
    EaseFunction easeFunction;

    public EaseFunction EaseFunction
    {
        get
        {
            if( easeFunction == null ) easeFunction = EaseManager.ToEaseFunction( ease );
            return easeFunction;
        }
    }

    public static void GetEaseAndDuration( DoTweenBlend blend, out float duration, out Ease ease )
    {
        if( blend == null ) 
        { 
            duration = .5f;
            ease = Ease.InOutCubic;
            return;
        }
        duration = blend.duration;
        ease = blend.ease;
    }
}
