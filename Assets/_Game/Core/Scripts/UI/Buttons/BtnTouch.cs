using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer.Unity;

namespace Game.UI
{
    /// <summary>
    /// Touch/mouse UI button without MMTouchButton dependency.
    /// Provides:
    /// - Click event (with drag threshold filtering)
    /// - Enable/Disable (interactable)
    /// - Pressed state scale animation via DOTween
    /// - Optional opacity via CanvasGroup
    /// - Optional "touch mode" (press on enter, release on exit)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class BtnTouch : UIElement, IInitializable, IButtonClickable,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
        IDragHandler, ISubmitHandler
    {
        [Header("Input")]
        [SerializeField] private float _dragThreshold = 15f;

        [Tooltip("If TRUE: normal mouse behavior (press only on pointer down). If FALSE: touch-like (press on enter, release on exit).")]
        [SerializeField] private bool _mouseMode = true;

        [Tooltip("Prevents repeated clicks within this time window (unscaled time).")]
        [SerializeField] private float _bufferDuration = 0f;

        [Header("Interactivity")]
        [SerializeField] private bool _useCanvasGroupOpacity = true;
        [SerializeField] private float _idleOpacity = 1f;
        [SerializeField] private float _pressedOpacity = 1f;
        [SerializeField] private float _disabledOpacity = 0.7f;

        [Header("Pressed Scale (DOTween)")]
        [SerializeField] private bool _usePressedScale = true;
        [SerializeField] private RectTransform _scaleTargetOverride;
        [SerializeField] private float _pressedScale = 0.92f;
        [SerializeField] private float _pressDuration = 0.08f;
        [SerializeField] private float _releaseDuration = 0.10f;
        [SerializeField] private Ease _pressEase = Ease.OutQuad;
        [SerializeField] private Ease _releaseEase = Ease.OutQuad;
        [SerializeField] private bool _useUnscaledTimeForTweens = true;

        [Header("Feedbacks")]
        [SerializeField] protected MMFeedbacks _onClickFeedback;

        public event Action OnClicked;

        protected bool _isInitialized;
        protected bool _interactable = true;

        private RectTransform _selfRect;
        private RectTransform _scaleTarget;
        private Vector3 _initialScale;
        private Selectable _selectable;
        private CanvasGroup _canvasGroup;
        private bool _pointerDown;
        private bool _pointerInside;
        private bool _dragDetected;
        private Vector2 _pointerDownPos;
        private float _lastClickTimestamp;
        private Tween _scaleTween;

        void IInitializable.Initialize()
        {
            if (_isInitialized) return;
            Initialize();
        }

        protected virtual void Initialize()
        {
            _selfRect = GetComponent<RectTransform>();

            _selectable = GetComponent<Selectable>();
            _canvasGroup = GetComponent<CanvasGroup>();

            _scaleTarget = _scaleTargetOverride != null ? _scaleTargetOverride : _selfRect;
            _initialScale = _scaleTarget.localScale;

            ApplyVisualIdle();

            _isInitialized = true;
        }

        public override void SetState(bool state)
        {
            if (state) Enable();
            else Disable();
        }

        public virtual void Enable()
        {
            _interactable = true;

            if (_selectable != null)
                _selectable.interactable = true;

            ApplyVisualIdle();
        }

        public virtual void Disable()
        {
            _interactable = false;

            if (_selectable != null)
                _selectable.interactable = false;

            CancelPress(resetVisual: true);
            ApplyVisualDisabled();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractableNow())
                return;

            if (_bufferDuration > 0f && Time.unscaledTime - _lastClickTimestamp < _bufferDuration)
                return;

            _pointerDown = true;
            _pointerInside = true;
            _dragDetected = false;
            _pointerDownPos = eventData.position;

            ApplyVisualPressed();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!_pointerDown)
                return;

            _pointerDown = false;

            // Always return visuals back
            ApplyVisualIdle();

            // Click condition
            if (_pointerInside && !_dragDetected && IsInteractableNow())
            {
                _lastClickTimestamp = Time.unscaledTime;
                OnClick();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;

            // Touch-like behavior: entering triggers press
            if (!_mouseMode && !_pointerDown && IsInteractableNow())
            {
                OnPointerDown(eventData);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;

            // Touch-like behavior: exiting cancels press (no click)
            if (!_mouseMode && _pointerDown)
            {
                CancelPress(resetVisual: true);
                ApplyVisualIdle();
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_pointerDown || _dragDetected)
                return;

            float delta = Vector2.Distance(_pointerDownPos, eventData.position);
            if (delta > _dragThreshold)
                _dragDetected = true;
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!IsInteractableNow())
                return;

            // Submit should behave like a click
            _lastClickTimestamp = Time.unscaledTime;
            OnClick();
        }

        protected virtual void OnClick()
        {
            if (_dragDetected)
                return;

            _onClickFeedback?.PlayFeedbacks();
            OnClicked?.Invoke();
        }

        protected virtual bool IsInteractableNow()
        {
            if (!_isInitialized)
                Initialize();

            if (!_interactable)
                return false;

            if (_selectable != null && !_selectable.interactable)
                return false;

            return true;
        }

        private void OnDisable()
        {
            // Ensure tweens are killed when object is disabled/destroyed
            KillScaleTween();
            if (_scaleTarget != null)
                _scaleTarget.localScale = _initialScale;

            _pointerDown = false;
            _pointerInside = false;
            _dragDetected = false;
        }

        private void OnDestroy()
        {
            KillScaleTween();
        }

        private void CancelPress(bool resetVisual)
        {
            _pointerDown = false;
            _dragDetected = false;

            if (resetVisual)
            {
                ApplyVisualIdle();
            }
        }

        private void ApplyVisualIdle()
        {
            if (_useCanvasGroupOpacity && _canvasGroup != null)
                _canvasGroup.alpha = _idleOpacity;

            if (_usePressedScale && _scaleTarget != null)
            {
                // Return to initial scale
                TweenScale(_initialScale, _releaseDuration, _releaseEase);
            }
        }

        private void ApplyVisualPressed()
        {
            if (_useCanvasGroupOpacity && _canvasGroup != null)
                _canvasGroup.alpha = _pressedOpacity;

            if (_usePressedScale && _scaleTarget != null)
            {
                Vector3 target = _initialScale * _pressedScale;
                TweenScale(target, _pressDuration, _pressEase);
            }
        }

        private void ApplyVisualDisabled()
        {
            if (_useCanvasGroupOpacity && _canvasGroup != null)
                _canvasGroup.alpha = _disabledOpacity;

            if (_usePressedScale && _scaleTarget != null)
            {
                // Usually disabled should stay at initial scale
                TweenScale(_initialScale, _releaseDuration, _releaseEase);
            }
        }

        private void TweenScale(Vector3 target, float duration, Ease ease)
        {
            KillScaleTween();

            // DOTween on UI: keep it simple, no overshoot unless you want it
            _scaleTween = _scaleTarget.DOScale(target, duration)
                .SetEase(ease)
                .SetUpdate(_useUnscaledTimeForTweens);
        }

        private void KillScaleTween()
        {
            if (_scaleTween == null) return;

            if (_scaleTween.IsActive())
                _scaleTween.Kill();

            _scaleTween = null;
        }
    }
}