using System;

namespace Game.Core.Input
{
    public interface IInputService
    {
        event Action ChargeStarted;
        event Action ChargeReleased;
        event Action ChargeCancelled;

        void CancelCharge();
    }
}
