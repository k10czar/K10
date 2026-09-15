#if NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif

using System.Collections.Generic;
using UnityEngine;


namespace K10.Platforms
{
    public class PlatformSpecificActivationHandler : PlatformSpecificBehaviour
    {
        enum EObjectDisablingMode
        {
            Disable,
            Destroy,
        }

        enum EObjectEnablingMode
        {
            Enable,
            DoNothing,
        }

#if NAUGHTY_ATTRIBUTES
        [InfoBox("Be careful when using this component!!\n\nEven though it has priority in script execution order, it still is applied on runtime and can't avoid Awake funcitons to be called on objects that start active in hierarchy. If you need to avoid this behaviour, consider disabling the object by default and using this component to enable it when needed.", EInfoBoxType.Warning)]
#endif
        [Space(5f)]
        [Header("Objects to Handle")]
        [SerializeField] private List<GameObject> _gameObjectsToDisable;
        [SerializeField] private List<Behaviour> _componentsToDisable;
        [Space(1.5f)]
        [SerializeField] private List<GameObject> _gameObjectsToEnable;
        [SerializeField] private List<Behaviour> _componentsToEnable;

        [Space(5f)]
        [Header("Handling Modes")]
        [SerializeField] private EObjectDisablingMode _disablingMode;
        [SerializeField] private EObjectEnablingMode _enablingMode;

        protected override void HandlePlatformsSpecificBehaviours(bool isContextValid)
        {
            foreach (var gameObject in _gameObjectsToEnable)
                HandleObject(gameObject, isContextValid);

            foreach (var gameObject in _gameObjectsToDisable)
                HandleObject(gameObject, !isContextValid);

            foreach (var component in _componentsToEnable)
                HandleComponent(component, isContextValid);

            foreach (var component in _componentsToDisable)
                HandleComponent(component, !isContextValid);
        }

        private void HandleObject(GameObject gameObject, bool isContextValid)
        {
            if (gameObject == null)
                return;

            if (isContextValid)
                EnableObject(gameObject);
            else
                DisableObject(gameObject);
        }

        private void HandleComponent(Behaviour component, bool isContextValid)
        {
            if (component == null)
                return;

            if (isContextValid)
                EnableComponent(component);
            else
                DisableComponent(component);
        }

        private void DisableObject(GameObject gameObject)
        {
            switch (_disablingMode)
            {
                case EObjectDisablingMode.Destroy:
                    Destroy(gameObject);
                    break;

                case EObjectDisablingMode.Disable:
                    gameObject.SetActive(false);
                    break;

                default:
                    break;
            }
        }

        private void DisableComponent(Behaviour component)
        {
            switch (_disablingMode)
            {
                case EObjectDisablingMode.Destroy:
                    Destroy(component);
                    break;

                case EObjectDisablingMode.Disable:
                    component.enabled = false;
                    break;

                default:
                    break;
            }
        }

        private void EnableObject(GameObject gameObject)
        {
            switch (_enablingMode)
            {
                case EObjectEnablingMode.Enable:
                    gameObject.SetActive(true);
                    break;

                case EObjectEnablingMode.DoNothing:
                    break;

                default:
                    break;
            }
        }

        private void EnableComponent(Behaviour component)
        {
            switch (_enablingMode)
            {
                case EObjectEnablingMode.Enable:
                    component.enabled = true;
                    break;

                case EObjectEnablingMode.DoNothing:
                    break;

                default:
                    break;
            }
        }
    }
}
