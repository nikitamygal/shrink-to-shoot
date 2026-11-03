using Game.Tools;
using MoreMountains.Tools;
using UnityEngine;
using VContainer;

namespace Game.Core.Input
{
    public class InputControl : InjectMonoBehaviour
    {
        [SerializeField] private MMTouchRepositionableJoystick _joystick;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Awake()
        {
            CacheBindings();
        }

        public virtual void SetMovement(Vector2 movement)
		{
            if (_inputService == null) return;
            
            _inputService.SetPrimaryMovement(movement);
		}

        private void CacheBindings()
        {
            if (_joystick != null) return;
            
            _joystick = GetComponent<MMTouchRepositionableJoystick>();
        }

        private void OnPermissionChanged(bool activate)
        {
            if (_joystick != null)
                _joystick.BackgroundCanvasGroup.gameObject.SetActive(activate);
        }

        private void OnEnable()
        {
            if (_inputService != null)
                _inputService.OnPermissionChanged += OnPermissionChanged;
        }

        private void OnDisable()
        {
            if (_inputService != null)
                _inputService.OnPermissionChanged -= OnPermissionChanged;
        }
    }
}
