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
        [SerializeField] private MMF_Player _damageFeedback;
        [SerializeField] private MMF_Player _destructionFeedback;
        [SerializeField] private Transform _visual;
        [SerializeField, Min(.5f), Tooltip("Safety limit for feedback accidentally configured to loop forever.")]
        private float _feedbackTimeout = 10f;

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
            EnsureDestructionFeedback();
        }

        private void EnsureDestructionFeedback()
        {
            if (_destructionFeedback != null) return;
            if (_visual == null) _visual = transform.Find("Model");
            if (_visual == null) return;

            // Old obstacle assets have no authored Feel player. Keep the fallback inside the
            // obstacle's lifetime and use the same Feel sequence as the Editor setup utility.
            var feedbackObject = new GameObject("Runtime Destruction Feedback");
            feedbackObject.transform.SetParent(transform, false);
            _destructionFeedback = feedbackObject.AddComponent<MMF_Player>();
            _destructionFeedback.AddFeedback(new MMF_Scale
            {
                Label = "Tree destruction",
                AnimateScaleTarget = _visual,
                AnimateScaleDuration = .35f,
                Mode = MMF_Scale.Modes.ToDestination,
                DestinationScale = Vector3.zero,
                DetermineScaleOnPlay = true,
                UniformScaling = true,
                RemapCurveZero = 0f,
                RemapCurveOne = 1f,
                AnimateScaleTweenX = new MoreMountains.Tools.MMTweenType(AnimationCurve.Linear(0f, 0f, 1f, 1f), "AnimateX")
            });
            _destructionFeedback.Initialization();
        }

        public void TakeDamage(int amount)
        {
            if (IsDestroying || !isActiveAndEnabled || amount <= 0) return;
            Health = Mathf.Max(0, Health - amount);
            if (Health > 0)
            {
                _damageFeedback?.PlayFeedbacks();
                return;
            }

            IsDestroying = true;
            if (_colliders == null) _colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < _colliders.Length; i++)
                if (_colliders[i] != null) _colliders[i].enabled = false;
            // Stop an in-progress damage pulse before the destruction feedback takes over its target.
            _damageFeedback?.StopFeedbacks();
            _destructionFeedback?.PlayFeedbacks();
            Destroyed?.Invoke(this);
            _events?.Trigger(new ObstacleDestroyed(this));
            _feedbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RemoveAfterFeedback(_feedbackCancellation.Token).Forget(Debug.LogException);
        }

        private async UniTask RemoveAfterFeedback(CancellationToken cancellation)
        {
            try
            {
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
            _damageFeedback?.StopFeedbacks();
            _destructionFeedback?.StopFeedbacks();
        }
    }
}
