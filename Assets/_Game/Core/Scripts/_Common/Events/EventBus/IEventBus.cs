using System;
using Game.Core.Interfaces;


namespace Game.Events
{
    public interface IEventBus
    {
        void Trigger<T>(T gameEvent) where T : IGameEvent;
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    }
}
