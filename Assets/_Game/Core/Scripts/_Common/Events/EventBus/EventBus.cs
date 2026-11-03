using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Interfaces;


namespace Game.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Trigger<T>(T gameEvent) where T : IGameEvent
        {
            if (_subscribers.TryGetValue(typeof(T), out var handlers))
                foreach (var handler in handlers.Cast<Action<T>>())
                    handler(gameEvent);
        }

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (!_subscribers.ContainsKey(typeof(T)))
                _subscribers[typeof(T)] = new List<Delegate>();
            _subscribers[typeof(T)].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (_subscribers.TryGetValue(typeof(T), out var handlers))
                handlers.Remove(handler);
        }
    }
}
