using DG.Tweening;
using UnityEngine;

namespace Game.UI.PopupWindow
{
    [CreateAssetMenu(menuName = "Game/Configs/PopupWindowConfig", fileName = "PopupWindowConfig")]
    public class PopupWindowConfig : ScriptableObject
    {
        [Header("Animation")]
        public float openDuration = 0.35f;
        public float closeDuration = 0.25f;

        public Ease openEase = Ease.OutBack;
        public Ease closeEase = Ease.InBack;

        [Header("Scale")]
        public Vector3 closedScale = Vector3.zero;
        public Vector3 openedScale = Vector3.one;

        [Header("Fade")]
        public float backgroundFade = 0.8f;
    }
}
