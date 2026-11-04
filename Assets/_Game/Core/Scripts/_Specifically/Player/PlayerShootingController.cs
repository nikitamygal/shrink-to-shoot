using Game.Configs;
using Game.Core.Input;
using Game.Gameplay;
using Game.Tools;
using UnityEngine;
using VContainer;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    public sealed class PlayerShootingController : InjectMonoBehaviour
    {
        [SerializeField] private PlayerMassController _mass;
        [SerializeField] private PlayerMovementController _movement;
        [SerializeField] private Projectile _projectilePrefab;

        private GameplayConfig _config;
        private IInputService _input;
        private IGameplayService _gameplay;
        private Projectile _projectile;
        private bool _charging;
        private bool _subscribed;
        private ChargeDependencyError _reportedDependencyError;

        private enum ChargeDependencyError
        {
            None,
            MissingConfig,
            MissingMass,
            MissingMovement,
            MissingProjectile,
            MissingPath,
            InvalidPath
        }

        public bool IsCharging => _charging;
        public bool HasActiveShot => _projectile != null;

        [Inject]
        public void Construct(GameplayConfig config, IInputService input, IGameplayService gameplay)
        {
            _config = config;
            _input = input;
            _gameplay = gameplay;
            if (_mass == null) _mass = GetComponent<PlayerMassController>();
            if (_movement == null) _movement = GetComponent<PlayerMovementController>();
            if (isActiveAndEnabled) Subscribe();
        }

        private void Subscribe()
        {
            if (_subscribed || _input == null) return;
            _input.ChargeStarted += BeginCharge;
            _input.ChargeReleased += ReleaseCharge;
            _input.ChargeCancelled += CancelCharge;
            _gameplay.GameState.OnStateChange += OnStateChanged;
            _subscribed = true;
        }

        private void Update()
        {
            if (_config == null) return;
            if (_charging)
            {
                if (!CanPlay()) { CancelCharge(); return; }
                _mass.UpdateTransfer(_config.ProjectileChargeSpeed * Time.deltaTime);
                // Critical transfer can synchronously change the game state and cancel this charge.
                if (_charging && _projectile != null)
                {
                    _projectile.SetMass(_mass.TransferredMass);
                    PositionChargedProjectile();
                }
            }
            else if (_projectile == null && CanPlay() && _movement != null && _movement.IsBlocked &&
                     _mass != null && !_mass.CanAffordMinimumProjectile)
            {
                _gameplay.GameState.ChangeState(GameStates.Lose);
            }
        }

        private bool CanPlay() => isActiveAndEnabled &&
            _gameplay != null && _gameplay.GameState.CurrentState == GameStates.Play;

        private void BeginCharge()
        {
            if (!CanPlay() || _charging || _projectile != null || !ValidateChargeDependencies()) return;
            if (!_mass.CanAffordMinimumProjectile)
            {
                if (_movement.IsBlocked) _gameplay.GameState.ChangeState(GameStates.Lose);
                return;
            }
            if (!_mass.BeginTransfer()) return;
            _charging = true;
            _projectile = Instantiate(_projectilePrefab);
            if (!_projectile.Initialize(_config))
            {
                CancelCharge();
                return;
            }
            _projectile.Finished += OnProjectileFinished;
            _projectile.SetMass(0f);
            PositionChargedProjectile();
        }

        private bool ValidateChargeDependencies()
        {
            ChargeDependencyError error;
            string message;
            if (_config == null)
            {
                error = ChargeDependencyError.MissingConfig;
                message = "Assign GameplayConfig on GameLifetimeScope and inject this player.";
            }
            else if (_mass == null)
            {
                error = ChargeDependencyError.MissingMass;
                message = "Assign PlayerMassController on this PlayerSphere.";
            }
            else if (_movement == null)
            {
                error = ChargeDependencyError.MissingMovement;
                message = "Assign PlayerMovementController on this PlayerSphere.";
            }
            else if (_projectilePrefab == null)
            {
                error = ChargeDependencyError.MissingProjectile;
                message = "Assign the Projectile prefab component in PlayerShootingController.";
            }
            else if (_movement.Path == null)
            {
                error = ChargeDependencyError.MissingPath;
                message = "PlayerMovementController has no LevelPath. Register the scene LevelPath in GameLifetimeScope and inject the player.";
            }
            else if (!_movement.Path.IsValid)
            {
                error = ChargeDependencyError.InvalidPath;
                message = "Assign valid ground waypoints on the scene LevelPath; its final waypoint is the shooting goal.";
            }
            else
            {
                _reportedDependencyError = ChargeDependencyError.None;
                return true;
            }

            if (_reportedDependencyError != error)
            {
                Debug.LogError("PlayerSphere cannot charge a shot. " + message, this);
                _reportedDependencyError = error;
            }
            return false;
        }

        private void PositionChargedProjectile()
        {
            Vector3 direction = _movement.Path.GoalPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < .0001f) direction = _movement.ForwardDirection;
            direction.Normalize();
            float shotRadius = _mass.TransferredRadius;
            _projectile.transform.position = transform.position + Vector3.up * Mathf.Max(_mass.CurrentRadius, shotRadius) +
                                             direction * (_mass.CurrentRadius + shotRadius + .08f);
        }

        private void ReleaseCharge()
        {
            if (!_charging) return;
            if (!CanPlay() || _projectile == null ||
                _mass.TransferredMass < SphereMass.FromRadius(_config.MinimumProjectileRadius))
            {
                CancelCharge();
                return;
            }
            PositionChargedProjectile();
            float mass = _mass.CommitTransfer();
            _charging = false;
            _projectile.SetMass(mass);
            Vector3 destination = _movement.Path.GoalPosition + Vector3.up * _projectile.Radius;
            _projectile.Launch(destination, transform.position + Vector3.up * _mass.CurrentRadius);
        }

        private void CancelCharge()
        {
            if (!_charging) return;
            _charging = false;
            // Crossing the critical threshold is a terminal spend; keep the loss visible at minimum size.
            if (_mass != null && _mass.IsCritical) _mass.CommitTransfer();
            else _mass?.CancelTransfer();
            DestroyActiveProjectile();
        }

        private void OnStateChanged()
        {
            if (_gameplay.GameState.CurrentState == GameStates.Play) return;
            CancelCharge();
            DestroyActiveProjectile();
        }

        private void OnProjectileFinished(Projectile projectile)
        {
            projectile.Finished -= OnProjectileFinished;
            if (_projectile == projectile) _projectile = null;
        }

        private void DestroyActiveProjectile()
        {
            if (_projectile == null) return;
            Projectile projectile = _projectile;
            _projectile = null;
            projectile.Finished -= OnProjectileFinished;
            projectile.Cancel();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable()
        {
            if (_subscribed)
            {
                _input.ChargeStarted -= BeginCharge;
                _input.ChargeReleased -= ReleaseCharge;
                _input.ChargeCancelled -= CancelCharge;
                _gameplay.GameState.OnStateChange -= OnStateChanged;
                _subscribed = false;
            }
            CancelCharge();
            DestroyActiveProjectile();
        }
    }
}
