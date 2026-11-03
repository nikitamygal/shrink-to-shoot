using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    [RequireComponent(typeof(MMTouchButton))]
    public class BtnMMTouch : UIElement, IButtonClickable
    {
        [SerializeField] private float _dragThreshold = 15f;

        [Header("Feedbacks")]
        [SerializeField] protected MMFeedbacks _onClickFeedback;

        public event Action OnClicked;
        
        protected MMTouchButton _btn;
        protected bool _isInitialized = false;

        private Vector2 _pointerDownPos;
        private bool _dragDetected;
        private bool _eventsBound;

        protected virtual void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (_isInitialized) return;
            Initialize();
        }

        protected virtual void Initialize()
        {
            if (_isInitialized)
                return;

            _btn = GetComponent<MMTouchButton>();
            _isInitialized = _btn != null;
        }

        public override void SetState(bool state)
        {
            if (state)
                Enable();
            else
                Disable();
        }

        public void Enable()
        {
            if (_btn == null) return;
            _btn.EnableButton();
            _btn.Interactable = true;
        }

        public void Disable()
        {
            if (_btn == null) return;
            _btn.DisableButton();
            _btn.Interactable = false;
        }

        protected virtual void OnClick()
        {
            if (_dragDetected)
                return;

            _onClickFeedback?.PlayFeedbacks();
            OnClicked?.Invoke();
        }

        private void OnButtonStateChanged(
            PointerEventData.FramePressState state,
            PointerEventData eventData)
        {
            if (eventData == null)
                return;

            switch (state)
            {
                case PointerEventData.FramePressState.Pressed:
                    _pointerDownPos = eventData.position;
                    _dragDetected = false;
                    break;

                case PointerEventData.FramePressState.Released:
                    // nothing here
                    break;
            }

            if (!_dragDetected)
            {
                float delta = Vector2.Distance(_pointerDownPos, eventData.position);
                if (delta > _dragThreshold)
                    _dragDetected = true;
            }
        }

        protected virtual void OnEnable()
        {
            Initialize();
            if (_btn == null || _eventsBound) return;

            _btn.ButtonReleased.AddListener(OnClick);
            _btn.ButtonStateChange += OnButtonStateChanged;
            _eventsBound = true;
        }

        protected virtual void OnDisable()
        {
            if (_btn == null || !_eventsBound) return;

            _btn.ButtonReleased.RemoveListener(OnClick);
            _btn.ButtonStateChange -= OnButtonStateChanged;
            _eventsBound = false;
        }

    }

}
