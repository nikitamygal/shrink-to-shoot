using System;

namespace Game.Core.Interfaces
{
    public interface IStateTransition
    {
        void Enter<TState>() where TState : IGameState;
        void Enter(Type stateType);
    }
}
