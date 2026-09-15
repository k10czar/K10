using UnityEngine;
using K10.Platforms;
using System.Collections.Generic;
using System;
#if NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif

namespace K10.Platforms
{
    public class PlatformSpecificScaleResizer : PlatformSpecificBehaviour
    {
        [Space(5)]
        [Header("Scale Config")]
        [SerializeField] bool _useMultiplierMode = true;
    #if NAUGHTY_ATTRIBUTES
        [ShowIf(nameof(_useMultiplierMode))]
    #endif
        [SerializeField, Range(0, 10f)] float _multiplier;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(nameof(_useMultiplierMode))]
    #endif
        [SerializeField] bool _dontChangeOnDefaultPlatforms = true;
        [SerializeField] bool _useVector;
        private bool DontUseVector => !_useVector;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(EConditionOperator.Or, nameof(_dontChangeOnDefaultPlatforms), nameof(_useMultiplierMode), nameof(DontUseVector))]
    #endif
        [SerializeField] float _defaultScaleFactor;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(EConditionOperator.Or, nameof(_useMultiplierMode), nameof(DontUseVector))]
    #endif
        [SerializeField] float _adjustedScaleFactor;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(EConditionOperator.Or, nameof(_dontChangeOnDefaultPlatforms), nameof(_useMultiplierMode), nameof(_useVector))]
    #endif
        [SerializeField] Vector3 _defaultScale;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(EConditionOperator.Or, nameof(_useMultiplierMode), nameof(_useVector))]
    #endif
        [SerializeField] Vector3 _adjustedScale;
        
        [SerializeField] List<Transform> _transforms;

        protected override void HandlePlatformsSpecificBehaviours(bool isContextValid)
        {
            if (!isContextValid && (_dontChangeOnDefaultPlatforms || _useMultiplierMode))
                return;
            
            if (_useMultiplierMode)
            {
                foreach (var transform in _transforms)
                    transform.localScale *= _multiplier;
            }
            else
            {
                Vector3 scale; 
                if (_useVector)
                    scale = isContextValid ? _adjustedScale : _defaultScale;
                else
                {
                    float scaleFactor = isContextValid ? _adjustedScaleFactor : _defaultScaleFactor;
                    scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                }

                foreach (var transform in _transforms)
                    transform.localScale = scale;
            }
        }
    }
}