using Game.Levels;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI
{
    public class UILevelNumber : UIElement
    {
        [SerializeField] private TextMeshProUGUI _textLevel;

        [Inject] private readonly ILevelService _levelService;

        private void Start()
        {
            SetLevelNumber(_levelService.GetLevelIndex() + 1);
        }

        private void SetLevelNumber(int level)
        {
            if (_textLevel == null) return;
            _textLevel.text = $"Level {level}";
        }

    }
}
