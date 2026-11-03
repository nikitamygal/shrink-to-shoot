using System;
using UnityEngine;


namespace Game.Core.Input
{
    public interface IInputService
    {
        Vector2 PrimaryMovement { get; }
        bool IsInputPermitted { get; }
        event Action<bool> OnPermissionChanged;

        void AllowInput(bool state);
        void SetPrimaryMovement(Vector2 movement);
    }
}
