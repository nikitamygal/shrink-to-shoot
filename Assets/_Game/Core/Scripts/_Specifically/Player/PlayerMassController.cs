using System;
using Game.Configs;
using Game.Gameplay;
using Game.Tools;
using UnityEngine;
using VContainer;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    public sealed class PlayerMassController : InjectMonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private SphereCollider _sphereCollider;

        private SphereMassTransfer _mass;
        private GameplayConfig _config;
        private IGameplayService _gameplay;
        private bool _criticalReported;

        public event Action Changed;
        public event Action CriticalMassReached;
        public float InitialMass => _mass?.InitialMass ?? 0f;
        public float CurrentMass => _mass?.CurrentMass ?? 0f;
        public float TransferredMass => _mass?.PendingMass ?? 0f;
        public float CurrentRadius => SphereMass.ToRadius(CurrentMass);
        public float TransferredRadius => SphereMass.ToRadius(TransferredMass);
        public bool IsTransferring => _mass != null && _mass.IsTransferring;
        public bool IsCritical => _mass != null && _mass.IsCritical;
        public bool CanAffordMinimumProjectile => _mass != null &&
            CurrentMass - _mass.MinimumMass > SphereMass.FromRadius(_config.MinimumProjectileRadius);

        [Inject]
        public void Construct(GameplayConfig config, IGameplayService gameplay)
        {
            _config = config;
            _gameplay = gameplay;
            _mass = new SphereMassTransfer(config.InitialPlayerRadius, config.MinimumPlayerRadius);
            CacheVisual();
            ApplySize();
        }

        private void Awake() => CacheVisual();

        private void CacheVisual()
        {
            if (_sphereCollider == null) _sphereCollider = GetComponentInChildren<SphereCollider>(true);
            if (_visual == null && _sphereCollider != null) _visual = _sphereCollider.transform;
        }

        public bool BeginTransfer() => _mass != null && _mass.Begin();

        public void UpdateTransfer(float massAmount)
        {
            if (_mass == null || !_mass.IsTransferring) return;
            _mass.Transfer(massAmount);
            ApplySize();
            if (_mass.IsCritical && !_criticalReported)
            {
                _criticalReported = true;
                CriticalMassReached?.Invoke();
                if (_gameplay.GameState.CurrentState == GameStates.Play)
                    _gameplay.GameState.ChangeState(GameStates.Lose);
            }
        }

        public float CommitTransfer() => _mass?.Commit() ?? 0f;

        public void CancelTransfer()
        {
            _mass?.Cancel();
            ApplySize();
        }

        private void ApplySize()
        {
            if (_mass == null) return;
            float radius = CurrentRadius;
            if (_visual != null)
            {
                _visual.localScale = Vector3.one * (radius * 2f);
                _visual.localPosition = new Vector3(0f, radius, 0f);
            }
            if (_sphereCollider != null)
            {
                _sphereCollider.radius = .5f;
                _sphereCollider.center = Vector3.zero;
            }
            Changed?.Invoke();
        }
    }
}
