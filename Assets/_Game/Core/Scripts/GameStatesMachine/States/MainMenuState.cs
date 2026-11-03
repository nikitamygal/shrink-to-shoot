using Game.Core.Interfaces;

namespace Game.GameFSM.States
{
    public class MainMenuState : IGameState
    {
        public MainMenuState()
        {
        }

        public void Enter()
        {
        }

        public void Exit()
        {
            // Cleanup menu-specific subscriptions, etc.
        }
    }
}
