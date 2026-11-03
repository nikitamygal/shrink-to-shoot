using System;
using UnityEngine;

namespace Game.Core.Input
{
    public class InputService : IInputService
    {
        public Vector2 PrimaryMovement => _primaryMovement;
        public bool IsInputPermitted  { get; private set; } = true;
        public event Action<bool> OnPermissionChanged;

        protected Vector2 _primaryMovement = Vector2.zero;

        public void AllowInput(bool state)
        {
            IsInputPermitted = state;
            _primaryMovement = Vector2.zero;
            OnPermissionChanged?.Invoke(state);
        }

        public void SetPrimaryMovement(Vector2 movement)
        {
            if (!IsInputPermitted)
                return;

            _primaryMovement = Vector2.ClampMagnitude(movement, 1f);
        }
    }
}
