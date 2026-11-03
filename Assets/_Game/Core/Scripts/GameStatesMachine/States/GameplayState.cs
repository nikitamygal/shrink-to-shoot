using Game.Core.Interfaces;

namespace Game.GameFSM.States
{
    public class GameplayState : IGameState
    {
        public void Enter()
        {
        }

        public void Exit()
        {
            // Stop gameplay loop, unsubscribe events, etc.
        }
    }
}
