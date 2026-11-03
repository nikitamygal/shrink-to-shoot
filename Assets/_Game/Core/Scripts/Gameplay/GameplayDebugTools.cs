using Game.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Game.Tools
{
    public class GameplayDebugTools : InjectMonoBehaviour
    {
        [Inject] private readonly IGameplayService _gameplayService;

        [Title("Win/Lose")]

        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
        public void ForceWin()
        {
            if (_gameplayService == null)
                return;

            Debug.Log("[GameplayDebugTools] Force WIN");
            _gameplayService.GameState.ChangeState(GameStates.Win);
        }

        [Button(ButtonSizes.Large), GUIColor(0.9f, 0.4f, 0.4f)]
        public void ForceLose()
        {
            if (_gameplayService == null)
                return;

            Debug.Log("[GameplayDebugTools] Force LOSE");
            _gameplayService.GameState.ChangeState(GameStates.Lose);
        }
    }

}
