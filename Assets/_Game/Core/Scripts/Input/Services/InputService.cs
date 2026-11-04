using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Game.Core.Input
{
    // VContainer ticks this once per frame. Gameplay receives only abstract charge events.
    public class InputService : IInputService, IInitializable, ITickable, IDisposable
    {
        public event Action ChargeStarted;
        public event Action ChargeReleased;
        public event Action ChargeCancelled;

        private bool _initialized;
        private bool _pressed;
        private bool _wasHeld;
        private bool _requiresRelease;
        private bool _disposed;
        private bool _hasFocus = true;
        private bool _chargeFromTouch;

        public void Initialize()
        {
            if (_initialized || _disposed) return;
            _initialized = true;
            _hasFocus = Application.isFocused;
            Application.focusChanged += OnFocusChanged;
            InputSystem.onDeviceChange += OnDeviceChanged;
        }

        public void Tick()
        {
            if (_disposed) return;
            Initialize();
            _hasFocus = Application.isFocused;
            var touchscreen = Touchscreen.current;
            bool touchHeld = touchscreen != null && touchscreen.primaryTouch.press.isPressed;
            bool held = touchHeld;
#if UNITY_EDITOR || UNITY_STANDALONE
            var mouse = Mouse.current;
            held |= mouse != null && mouse.leftButton.isPressed;
#endif
            bool touchCancelled = touchscreen != null &&
                touchscreen.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            if (_pressed && touchHeld) _chargeFromTouch = true;
            if (!_hasFocus || (touchCancelled && _pressed && _chargeFromTouch))
            {
                _wasHeld = held;
                CancelCharge();
                return;
            }
            if (_requiresRelease)
            {
                _wasHeld = held;
                if (!held) _requiresRelease = false;
                return;
            }
            if (held == _wasHeld) return;
            _wasHeld = held;
            if (held)
            {
                _chargeFromTouch = touchHeld;
                _pressed = true;
                ChargeStarted?.Invoke();
            }
            else if (_pressed)
            {
                _pressed = false;
                ChargeReleased?.Invoke();
            }
        }

        public void CancelCharge()
        {
            // A cancelled hold must be released before a new charge can start.
            _requiresRelease = true;
            if (!_pressed) return;
            _pressed = false;
            ChargeCancelled?.Invoke();
        }

        private void OnFocusChanged(bool focused)
        {
            _hasFocus = focused;
            if (!focused) CancelCharge();
        }

        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Disconnected || change == InputDeviceChange.Removed ||
                change == InputDeviceChange.Disabled) CancelCharge();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CancelCharge();
            if (!_initialized) return;
            Application.focusChanged -= OnFocusChanged;
            InputSystem.onDeviceChange -= OnDeviceChanged;
        }
    }
}
