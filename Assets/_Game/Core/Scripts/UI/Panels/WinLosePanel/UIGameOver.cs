using Game.GameFSM;
using Game.GameFSM.States;
using Game.Levels;
using VContainer;

namespace Game.UI
{
    public class UIGameOverPanel : UIPanel
    {
        [Inject] protected readonly ILevelService _levelService;
        [Inject] protected readonly GameStateMachine _stateMachine;

        protected virtual void OnRewardsClaimed()
        {
            Hide();
            _stateMachine.Enter<GameplayState>();
        }
    }
}
