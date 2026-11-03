using Game.UI;
using Game.UI.PopupWindow;
using UnityEngine;
using VContainer;

namespace Game.UI
{
    public class BtnOpenPopup : BtnTouch
    {
        [Header("Popup")]
        [SerializeField] private UIPanel _popupPrefab;

        [Inject] private readonly IPopupWindowService _popupWindowService;
        
        protected override void OnClick()
        {
            base.OnClick();
            _popupWindowService?.Open(_popupPrefab);
        }
    }
}
