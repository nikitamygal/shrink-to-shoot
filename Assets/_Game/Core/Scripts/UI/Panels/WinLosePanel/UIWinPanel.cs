using Game.Levels;
using UnityEngine;
using VContainer;

namespace Game.UI
{
    public class UIWinPanel : UIGameOverPanel
    {
        [SerializeField] private BtnTouch _nextBtn;

        [Inject] private readonly ILevelService _levelService;

        protected override BtnTouch ActionButton => _nextBtn;

        protected override void OnContinue()
        {
            _levelService.IncrementLevelIndex();
            base.OnContinue();
        }
    }
}
