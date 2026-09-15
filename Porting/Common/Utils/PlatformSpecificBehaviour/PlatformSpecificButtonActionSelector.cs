using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace K10.Platforms
{
    public class PlatformSpecificButtonActionSelector : PlatformSpecificBehaviour
    {
        enum EButtonEventSetMode
        {
            Add,
            Override,
        }

        [Space(5)]
        [Header("Button Config")]
        [SerializeField] private Button _button;
        [SerializeField] private EButtonEventSetMode _eventSetMode;
        [SerializeField] private UnityEvent _onClickDefaultEvents;
        [SerializeField] private UnityEvent _onClickSpecificEvents;

        protected override void HandlePlatformsSpecificBehaviours(bool isContextValid)
        {
            UnityEvent actionsToAdd = isContextValid ? _onClickSpecificEvents : _onClickDefaultEvents;
            switch (_eventSetMode)
            {
                case EButtonEventSetMode.Override:
                    _button.onClick.RemoveAllListeners();
                    AddActionsToButton(actionsToAdd);
                    break;
                case EButtonEventSetMode.Add:
                    AddActionsToButton(actionsToAdd);
                    break;
                default:
                    break;
            }

        }

        private void AddActionsToButton(UnityEvent actions)
        {
            if (_button == null)
                return;

            _button.onClick.AddListener(() => actions?.Invoke());
        }
    }
}