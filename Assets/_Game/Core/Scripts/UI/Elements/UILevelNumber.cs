using DG.Tweening;
using Game.Levels;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UILevelNumber : UIElement
    {
        [SerializeField] private TextMeshProUGUI _textLevel;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private float _fadeDelay = 2f;

        [Inject] private readonly ILevelService _levelService;

        private void Start()
        {
            SetLevelNumber(_levelService.GetLevelIndex() + 1);
            GetComponent<CanvasGroup>()
                .DOFade(0f, _fadeDuration)
                .SetDelay(_fadeDelay)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void SetLevelNumber(int level)
        {
            if (_textLevel == null) return;
            _textLevel.text = $"Level {level}";
        }

    }
}
