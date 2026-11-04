using System.Collections.Generic;
using DG.Tweening;
using Game.GameFSM;
using Game.GameFSM.States;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIGameOverPanel : UIPanel, IInitializable
    {
        [Header("Visuals")]
        [SerializeField] private Image _emojiImage;
        [SerializeField] private List<Sprite> _emojiSprites = new();
        [SerializeField, Min(0.01f)] private float _emojiDuration = 0.5f;
        [SerializeField] private Ease _emojiEase = Ease.OutBounce;
        [SerializeField] private Image _titleImage;
        [FormerlySerializedAs("_perfectDuration")]
        [SerializeField, Min(0.01f)] private float _titleDuration = 0.5f;
        [SerializeField] private Ease _titleEase = Ease.OutElastic;
        [SerializeField, Min(0.01f)] private float _appearDuration = 0.5f;

        [Header("Feedbacks")]
        [SerializeField] private MMFeedbacks _onShowFeedbacks;

        private CanvasGroup _canvasGroup;
        private Vector3 _emojiScale;
        private Vector3 _titleScale;
        private Sequence _appearSequence;
        private bool _transitionStarted;

        [Inject] private readonly GameStateMachine _stateMachine;

        protected abstract BtnTouch ActionButton { get; }

        void IInitializable.Initialize()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _emojiScale = _emojiImage != null ? _emojiImage.transform.localScale : Vector3.one;
            _titleScale = _titleImage != null ? _titleImage.transform.localScale : Vector3.one;
        }

        public override void Show()
        {
            _transitionStarted = false;
            ActionButton?.Enable();
            StopAppearance();
            SetRandomSprite(_emojiImage, _emojiSprites);

            base.Show();
            _onShowFeedbacks?.PlayFeedbacks();
            PlayAppearance();
        }

        private void PlayAppearance()
        {
            _canvasGroup.alpha = 0f;
            _appearSequence = DOTween.Sequence().SetUpdate(true);
            _appearSequence.Append(_canvasGroup.DOFade(1f, _appearDuration * 0.5f));
            if (_emojiImage != null)
            {
                _emojiImage.transform.localScale = _emojiScale * 0.25f;
                _appearSequence.Insert(0f, _emojiImage.transform.DOScale(_emojiScale, _emojiDuration).SetEase(_emojiEase));
            }
            if (_titleImage != null)
            {
                _titleImage.transform.localScale = _titleScale * 0.25f;
                _appearSequence.Insert(0.1f, _titleImage.transform.DOScale(_titleScale, _titleDuration).SetEase(_titleEase));
            }
        }

        private static void SetRandomSprite(Image image, List<Sprite> sprites)
        {
            if (image == null || sprites == null || sprites.Count == 0)
                return;

            Sprite sprite = sprites[Random.Range(0, sprites.Count)];
            if (sprite != null) image.sprite = sprite;
        }

        protected virtual void OnEnable()
        {
            if (ActionButton != null)
                ActionButton.OnClicked += Continue;
        }

        protected virtual void OnDisable()
        {
            if (ActionButton != null)
                ActionButton.OnClicked -= Continue;

            StopAppearance();
            _onShowFeedbacks?.StopFeedbacks();
        }

        private void Continue()
        {
            if (_transitionStarted)
                return;

            _transitionStarted = true;
            ActionButton?.Disable();
            OnContinue();
        }

        protected virtual void OnContinue()
        {
            _stateMachine.Enter<GameplayState>();
        }

        private void StopAppearance()
        {
            _appearSequence?.Kill();
            _appearSequence = null;
            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = 1f;
            if (_emojiImage != null) _emojiImage.transform.localScale = _emojiScale;
            if (_titleImage != null) _titleImage.transform.localScale = _titleScale;
        }
    }
}
