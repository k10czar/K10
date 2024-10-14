using DG.Tweening;
using UnityEngine;

[CreateAssetMenu( fileName = "DoTweenBlend", menuName = "DOTween/DoTweenBlend", order = 0 )]
public class DoTweenBlend : ScriptableObject
{
    [SerializeField] float duration = .5f;
    [SerializeField] Ease ease = Ease.InOutCubic;

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
