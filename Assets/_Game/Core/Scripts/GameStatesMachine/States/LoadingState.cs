using System;
using Game.Core.Interfaces;
using Game.Services;

namespace Game.GameFSM.States
{
    public class LoadingState : IGameState
    {
        private readonly ISceneService _sceneService;
        private readonly IStateTransitionInternal _transition;

        private string _nextScene;
        private Type _nextState;

        public LoadingState(
            ISceneService sceneService,
            IStateTransitionInternal transition)
        {
            _sceneService = sceneService;
            _transition = transition;
        }

        public void Load(string scene, Type nextState)
        {
            _nextScene = scene;
            _nextState = nextState;
            RunAsync();
        }

        private async void RunAsync()
        {
            await _sceneService.LoadSceneAsync(_nextScene);

            _transition.EnterInternal(_nextState);
        }

        public void Enter() { }
        public void Exit() { }
    }

}
