using DG.Tweening;
using Game.Configs;
using Game.Gameplay;
using Game.Tools;
using MoreMountains.Feedbacks;
using UnityEngine;
using VContainer;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class EndPoint : InjectMonoBehaviour
    {
        [SerializeField] private MMFeedbacks _exitFeedback;

        private GameplayConfig _config;
        private IGameplayService _gameplay;
        private Sequence _absorption;
        private bool _entered;

        [Inject]
        public void Construct(GameplayConfig config, IGameplayService gameplay)
        {
            _config = config;
            _gameplay = gameplay;
        }

        private void OnTriggerEnter(Collider other) => TryEnter(other);
        private void OnTriggerStay(Collider other) => TryEnter(other);

        private void TryEnter(Collider other)
        {
            if (_entered || _config == null || _gameplay.GameState.CurrentState != GameStates.Play) return;
            PlayerMovementController player = other.GetComponentInParent<PlayerMovementController>();
            if (player == null || !player.isActiveAndEnabled || !player.BeginExit()) return;

            _entered = true;
            // Cancels the current charge and projectile before animating the player's scale.
            PlayerShootingController shooting = player.GetComponent<PlayerShootingController>();
            if (shooting != null) shooting.enabled = false;

            Transform sphere = player.transform;
            float duration = Mathf.Max(.01f, _config.ExitDuration);
            Vector3 stretchedScale = Vector3.Scale(sphere.localScale, new Vector3(.8f, 1.2f, .8f));
            _absorption = DOTween.Sequence()
                .Insert(0f, sphere.DOMove(transform.position, duration).SetEase(Ease.InCubic))
                .Insert(0f, sphere.DOLocalRotate(Vector3.up * _config.ExitSpinDegrees, duration,
                    RotateMode.LocalAxisAdd).SetEase(Ease.InQuad))
                .Insert(0f, sphere.DOScale(stretchedScale, duration * .2f).SetEase(Ease.OutQuad))
                .Insert(duration * .2f, sphere.DOScale(Vector3.zero, duration * .8f).SetEase(Ease.InCubic))
                .SetLink(sphere.gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(() =>
                {
                    _absorption = null;
                    if (_gameplay.GameState.CurrentState == GameStates.Play)
                        _gameplay.GameState.ChangeState(GameStates.Win);
                });
            _exitFeedback?.PlayFeedbacks();
        }

        private void OnDisable()
        {
            _absorption?.Kill();
            _absorption = null;
            _exitFeedback?.StopFeedbacks();
        }
    }
}
