using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Configs;
using Game.Events;
using Game.Events.Gameplay;
using Game.Tools;
using MoreMountains.Feedbacks;
using UnityEngine;
using VContainer;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    public sealed class Obstacle : InjectMonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("Zero uses GameplayConfig.DefaultObstacleHealth.")]
        private int _healthOverride;
        [SerializeField, Min(.5f), Tooltip("Safety limit for feedback accidentally configured to loop forever.")]
        private float _feedbackTimeout = 10f;

        [Header("Feedbacks")]
        [SerializeField] private MMFeedbacks _damageFeedback;
        [SerializeField] private MMFeedbacks _destructionFeedback;

        private Collider[] _colliders;
        private IEventBus _events;
        private CancellationTokenSource _feedbackCancellation;
        public event Action<Obstacle> Destroyed;
        public int Health { get; private set; } = 1;
        public bool IsDestroying { get; private set; }

        [Inject]
        public void Construct(GameplayConfig config, IEventBus events)
        {
            _events = events;
            Health = Mathf.Max(1, _healthOverride > 0 ? _healthOverride : config.DefaultObstacleHealth);
        }

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        public void TakeDamage(int amount)
        {
            if (IsDestroying || !isActiveAndEnabled || amount <= 0) return;
            Health = Mathf.Max(0, Health - amount);
            _damageFeedback?.PlayFeedbacks();
            if (Health > 0) return;

            IsDestroying = true;
            if (_colliders == null) _colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < _colliders.Length; i++)
                if (_colliders[i] != null) _colliders[i].enabled = false;
            Destroyed?.Invoke(this);
            _events?.Trigger(new ObstacleDestroyed(this));
            _feedbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RemoveAfterFeedback(_feedbackCancellation.Token).Forget(Debug.LogException);
        }

        private async UniTask RemoveAfterFeedback(CancellationToken cancellation)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.5f), cancellationToken: cancellation);
                if (_damageFeedback != null && _damageFeedback.IsPlaying) _damageFeedback.StopFeedbacks();
                _destructionFeedback?.PlayFeedbacks();
                // Let Feel's first update start, including a configured initial delay.
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
                float elapsed = 0f;
                while (_destructionFeedback != null && _destructionFeedback.IsPlaying &&
                       elapsed < Mathf.Max(.5f, _feedbackTimeout))
                {
                    elapsed += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
                }
                if (!cancellation.IsCancellationRequested && this != null) Destroy(gameObject);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            finally
            {
                _feedbackCancellation?.Dispose();
                _feedbackCancellation = null;
            }
        }

        private void OnDisable()
        {
            _feedbackCancellation?.Cancel();
            if (_damageFeedback != null && _damageFeedback.IsPlaying) _damageFeedback.StopFeedbacks();
            if (_destructionFeedback != null && _destructionFeedback.IsPlaying) _destructionFeedback.StopFeedbacks();
        }
    }
}
