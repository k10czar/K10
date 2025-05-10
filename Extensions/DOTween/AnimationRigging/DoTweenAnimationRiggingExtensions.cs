using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public static class DoTweenAnimationRiggingExtensions
{
    /// <summary>Tweens a Animation's Rig weitgh to the given value.
    /// Also stores the Rig as the tween's target so it can be used for filtered operations</summary>
    /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
    public static TweenerCore<float, float, FloatOptions> DOFade(this UnityEngine.Animations.Rigging.Rig target, float endValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.weight, x => target.weight = x, endValue, duration);
        t.SetTarget(target);
        return t;
    }
}

