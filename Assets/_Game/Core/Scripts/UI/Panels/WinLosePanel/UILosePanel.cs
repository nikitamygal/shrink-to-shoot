using UnityEngine;

namespace Game.UI
{
    public class UILosePanel : UIGameOverPanel
    {
        [SerializeField] private BtnTouch _retryBtn;

        protected override BtnTouch ActionButton => _retryBtn;
    }
}
