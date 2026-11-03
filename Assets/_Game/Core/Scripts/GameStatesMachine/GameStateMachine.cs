using System;
using System.Collections.Generic;
using Game.Core.Interfaces;
using Game.GameFSM.States;
using VContainer;

namespace Game.GameFSM
{
    public class GameStateMachine : IStateTransition, IStateTransitionInternal
    {
        private readonly IStateSceneMap _sceneMap;
        private readonly IObjectResolver _resolver;

        private readonly Dictionary<Type, IGameState> _states = new();
        private IGameState _activeState;

        public GameStateMachine(
            IStateSceneMap sceneMap,
            IObjectResolver resolver)
        {
            _sceneMap = sceneMap;
            _resolver = resolver;
        }

        public void Enter<T>() where T : IGameState
        {
            Enter(typeof(T));
        }

        public void Enter(Type stateType)
        {
            if (_sceneMap.TryGetScene(stateType, out var sceneName))
            {
                // route through Loading
                var loading = GetState<LoadingState>();
                _activeState?.Exit();
                _activeState = loading;
                loading.Load(sceneName, stateType);
                return;
            }

            EnterInternal(stateType);
        }

        void IStateTransitionInternal.EnterInternal(Type stateType)
        {
            EnterInternal(stateType);
        }

        internal void EnterInternal(Type stateType)
        {
            var state = GetState(stateType);
            _activeState?.Exit();
            _activeState = state;
            state.Enter();
        }

        private T GetState<T>() where T : IGameState
        {
            return (T)GetState(typeof(T));
        }

        private IGameState GetState(Type type)
        {
            if (_states.TryGetValue(type, out var state))
                return state;

            state = (IGameState)_resolver.Resolve(type);
            _states[type] = state;
            return state;
        }
    }
}
