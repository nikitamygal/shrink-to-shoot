using Game.Core.Interfaces;

namespace Game.GameFSM.States
{
    public class TestState : IGameState
    {
        public TestState()
        {
        }

        public void Enter()
        {
            // Scene is already loaded by LoadingState.
            // Put UI/menu initialization logic here if needed.
        }

        public void Exit()
        {
            // Cleanup menu-specific subscriptions, etc.
        }
    }
}
