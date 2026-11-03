using System;

namespace Game.Core.Interfaces
{
    public interface IStateTransitionInternal
    {
        void EnterInternal(Type stateType);
    }
}
