using Game.Core.Interfaces;
using UnityEngine.SceneManagement;

namespace Game.GameFSM.States
{
    public class EntryState : IGameState
    {
        private readonly IStateTransition _transition;
        private readonly IStateTransitionInternal _internalTransition;
        private readonly IStateSceneMap _sceneMap;

        public EntryState(
                IStateTransition transition,
                IStateTransitionInternal internalTransition,
                IStateSceneMap sceneMap)
            {
                _transition = transition;
                _internalTransition = internalTransition;
                _sceneMap = sceneMap;
            }

        public void Enter()
        {
            string activeScene = SceneManager.GetActiveScene().name;

            // Direct-play case
            if (_sceneMap.TryGetStateForScene(activeScene, out var stateType))
            {
                _internalTransition.EnterInternal(stateType);
                return;
            }

            _transition.Enter<GameplayState>();
        }

        public void Exit() { }
    }
}
