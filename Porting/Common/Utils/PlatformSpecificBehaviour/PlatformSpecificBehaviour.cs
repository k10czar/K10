using System.Collections.Generic;
using UnityEngine;
#if NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif

namespace K10.Platforms
{
    public abstract class PlatformSpecificBehaviour : MonoBehaviour
    {
        enum EPlatformFilterType
        {
            Platform,
            PlatformFamily,
            PlatformType,
        }
        
        [Header("Conditions")]
        [SerializeField] private EPlatformFilterType _filterType;


#if NAUGHTY_ATTRIBUTES
        [EnumFlags, ShowIf("IsPlatformmSpecific")] 
#endif
        [SerializeField] private EPlatform _platforms;
#if NAUGHTY_ATTRIBUTES
        [EnumFlags, ShowIf("IsPlatformmFamilySpecific")] 
#endif
        [SerializeField] private EPlatformFamily _platformFamilies;
#if NAUGHTY_ATTRIBUTES
        [EnumFlags, ShowIf("IsPlatformmTypeSpecific")] 
#endif
        [SerializeField] private EPlatformType _platformTypes;
        [Tooltip("Does not apply to Steam Deck")]
        [SerializeField] private bool _useInvertedCondition;

        // TODO: Steam Deck
//         [Header("Steam Deck")]
//         [SerializeField] private bool _useSpecificBehaviourOnSteamDeck;
// #if NAUGHTY_ATTRIBUTES
//         [ShowIf(nameof(_useSpecificBehaviourOnSteamDeck))] 
// #endif
//         [SerializeField] private bool _acitveOnSteamDeck;

        protected bool IsPlatformmSpecific() { return _filterType == EPlatformFilterType.Platform; }
        protected bool IsPlatformmFamilySpecific() { return _filterType == EPlatformFilterType.PlatformFamily; }
        protected bool IsPlatformmTypeSpecific() { return _filterType == EPlatformFilterType.PlatformType; }

		protected void Awake() 
        {
            HandlePlatformsSpecificBehaviours(IsInValidContext());
        }

        protected abstract void HandlePlatformsSpecificBehaviours(bool isContextValid);

        protected virtual bool IsInValidContext()
        {
            bool isInValidContext;
            switch (_filterType)
            {
                case EPlatformFilterType.Platform:
                     isInValidContext = (PlatformManager.GetPlatform() & _platforms) != 0;
                     break;
                case EPlatformFilterType.PlatformFamily:
                    isInValidContext = (PlatformManager.GetPlatformFamily() & _platformFamilies) != 0;
                     break;
                case EPlatformFilterType.PlatformType:
                    isInValidContext = (PlatformManager.GetPlatformType() & _platformTypes) != 0;
                     break;
                default:
                    return true;
            }

            if (_useInvertedCondition)
                isInValidContext = !isInValidContext;

            // TODO: Steam Deck
            // if (_useSpecificBehaviourOnSteamDeck && PlatformManager.IsOnSteamDeck())
            //     isInValidContext = _acitveOnSteamDeck;

            return isInValidContext;
        }
    }
}
