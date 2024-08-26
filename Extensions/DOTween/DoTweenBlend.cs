using DG.Tweening;
using UnityEngine;

[CreateAssetMenu( fileName = "DoTweenBlend", menuName = "DOTween/DoTweenBlend", order = 0 )]
public class DoTweenBlend : ScriptableObject
{
    [SerializeField] float duration = .5f;
    [SerializeField] Ease ease = Ease.InOutQuad;

    public static void GetBlendAndDuration( DoTweenBlend blend, out float duration, out Ease ease )
    {
        if( blend == null ) 
        { 
            duration = .5f;
            ease = Ease.InOutQuad;
            return;
        }
        duration = blend.duration;
        ease = blend.ease;
    }
}
