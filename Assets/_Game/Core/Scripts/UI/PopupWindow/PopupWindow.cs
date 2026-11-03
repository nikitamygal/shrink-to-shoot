using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.PopupWindow
{
    public class PopupWindow : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PopupWindowConfig _config;

        [Header("Refs")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private BtnTouch _closeButton;
        [SerializeField] private RectTransform _windowRoot;
        [SerializeField] private RectTransform _popupRect;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private CanvasGroup _windowCanvasGroup;
        [SerializeField] private CanvasGroup _background;
        [SerializeField] private CanvasGroup _dimmed;

        [Header("Auto Size")]
        [SerializeField] private bool _autoResizeByContent = true;
        [SerializeField] private float _minWindowHeight = 300f;
        [SerializeField] private float _maxWindowHeight = 1400f;

        private Tween _tween;
        private Button _dimmedButton;
        private Action _onClose;

        public RectTransform ContentRoot => _contentRoot;

        public void BindClose(Action onClose)
        {
            _onClose -= onClose;
            _onClose += onClose;

            _closeButton.OnClicked -= HandleCloseClicked;
            _closeButton.OnClicked += HandleCloseClicked;

            if (_dimmed == null || _dimmedButton != null)
                return;

            _dimmedButton = _dimmed.GetComponent<Button>();
            if (_dimmedButton == null)
            {
                _dimmedButton = _dimmed.gameObject.AddComponent<Button>();
                _dimmedButton.transition = Selectable.Transition.None;
                _dimmedButton.navigation = new Navigation { mode = Navigation.Mode.None };
            }

            _dimmedButton.onClick.AddListener(HandleCloseClicked);
        }

        private void HandleCloseClicked()
        {
            _onClose?.Invoke();
        }

        public void SetTitle(string title)
        {
            if (_titleText == null)
                return;

            bool hasTitle = !string.IsNullOrEmpty(title);
            _titleText.gameObject.SetActive(hasTitle);
            if (hasTitle) _titleText.text = title;
        }

        public void RefreshLayoutAndResize(RectTransform contentRect)
        {
            if (!_autoResizeByContent)
                return;

            if (_popupRect == null || _contentRoot == null || contentRect == null)
                return;

            Vector3 cachedScale = _windowRoot.localScale;
            _windowRoot.localScale = Vector3.one;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_popupRect);
            Canvas.ForceUpdateCanvases();

            float contentHeight = GetContentHeight(contentRect);

            float topInset = Mathf.Abs(_contentRoot.offsetMax.y);
            float bottomInset = Mathf.Abs(_contentRoot.offsetMin.y);

            float targetHeight = contentHeight + topInset + bottomInset;
            targetHeight = Mathf.Clamp(targetHeight, _minWindowHeight, _maxWindowHeight);

            _popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_popupRect);
            Canvas.ForceUpdateCanvases();

            _windowRoot.localScale = cachedScale;
        }

        private float GetContentHeight(RectTransform contentRect)
        {
            float preferredHeight = LayoutUtility.GetPreferredHeight(contentRect);
            if (preferredHeight > 1f)
                return preferredHeight;

            LayoutElement layoutElement = contentRect.GetComponent<LayoutElement>();
            if (layoutElement != null && layoutElement.preferredHeight > 1f)
                return layoutElement.preferredHeight;

            float rectHeight = contentRect.rect.height;
            if (rectHeight > 1f)
                return rectHeight;

            return 0f;
        }

        public void PlayOpen()
        {
            _tween?.Kill();

            _windowRoot.localScale = _config.closedScale;
            _background.alpha = 0f;
            _windowCanvasGroup.alpha = 0f;
            _windowCanvasGroup.blocksRaycasts = false;

            _tween = DOTween.Sequence()
                .Append(_windowRoot
                    .DOScale(_config.openedScale, _config.openDuration)
                    .SetEase(_config.openEase))
                .Join(_windowCanvasGroup
                    .DOFade(1f, _config.openDuration))
                .Join(_background
                    .DOFade(_config.backgroundFade, _config.openDuration))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _windowCanvasGroup.blocksRaycasts = true;
                });
        }

        public void PlayClose(Action onComplete)
        {
            _tween?.Kill();

            _tween = DOTween.Sequence()
                .Append(_windowRoot
                    .DOScale(_config.closedScale, _config.closeDuration)
                    .SetEase(_config.closeEase))
                .Join(_windowCanvasGroup
                    .DOFade(0f, _config.closeDuration))
                .Join(_background
                    .DOFade(0f, _config.closeDuration))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _windowCanvasGroup.blocksRaycasts = false;
                    onComplete?.Invoke();
                });
        }

        public void ClearContent()
        {
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_contentRoot.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.OnClicked -= HandleCloseClicked;

            if (_dimmedButton != null)
                _dimmedButton.onClick.RemoveListener(HandleCloseClicked);

            _tween?.Kill();
        }
    }
}
