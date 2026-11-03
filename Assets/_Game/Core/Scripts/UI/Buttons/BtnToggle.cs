using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI
{
    public enum BtnToggleState
    {
        Inactive,
        Active,
        Paused
    }

    public class BtnToggle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Text")]
        [SerializeField] private string _textPrefix = "Auto buy";

        [Header("Sprites")]
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _inactiveSprite;
        [SerializeField] private Sprite _pausedSprite;

        [Header("Colors")]
        [SerializeField] private Color _activeColor = Color.green;
        [SerializeField] private Color _inactiveColor = Color.gray;
        [SerializeField] private Color _pausedColor = Color.yellow;

        [Header("State Labels")]
        [SerializeField] private string _activeStateText = "ON";
        [SerializeField] private string _inactiveStateText = "OFF";
        [SerializeField] private string _pausedStateText = "PAUSE";

        [Header("Events")]
        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        private BtnToggleState _state;

        public bool IsActive => _state == BtnToggleState.Active;
        public BtnToggleState State => _state;

        /// <summary>
        /// Activates button state
        /// </summary>
        public void ActivateButton()
        {
            SetState(BtnToggleState.Active);
        }

        /// <summary>
        /// Deactivates button state
        /// </summary>
        public void DeactivateButton()
        {
            SetState(BtnToggleState.Inactive);
        }

        public void PauseButton()
        {
            SetState(BtnToggleState.Paused);
        }

        /// <summary>
        /// Toggle button state
        /// </summary>
        public void ToggleButton()
        {
            if (_state == BtnToggleState.Active)
                DeactivateButton();
            else
                ActivateButton();
        }

        public void SetState(BtnToggleState state)
        {
            _state = state;
            UpdateVisual();

            switch (_state)
            {
                case BtnToggleState.Active:
                    OnActivated?.Invoke();
                    break;
                case BtnToggleState.Inactive:
                    OnDeactivated?.Invoke();
                    break;
            }
        }

        private void UpdateVisual()
        {
            if (_image != null)
            {
                _image.sprite = GetSpriteForState();
            }

            if (_label != null)
                _label.color = GetTextColorForState();

            if (_label != null)
                _label.text = $"{_textPrefix}: {GetStateText()}";
        }

        private Sprite GetSpriteForState()
        {
            return _state switch
            {
                BtnToggleState.Active => _activeSprite,
                BtnToggleState.Paused => _pausedSprite != null ? _pausedSprite : _activeSprite,
                _ => _inactiveSprite
            };
        }

        private Color GetTextColorForState()
        {
            return _state switch
            {
                BtnToggleState.Active => _activeColor,
                BtnToggleState.Paused => _pausedColor,
                _ => _inactiveColor
            };
        }

        private string GetStateText()
        {
            return _state switch
            {
                BtnToggleState.Active => _activeStateText,
                BtnToggleState.Paused => _pausedStateText,
                _ => _inactiveStateText
            };
        }
    }
}
