using System;
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
    public class PlayerMovementController : InjectMonoBehaviour
    {
        [SerializeField] private PlayerMassController _mass;
        [SerializeField] private MMFeedbacks _jumpFeedback;

        private LevelPath _path;
        private GameplayConfig _config;
        private IGameplayService _gameplay;
        private Tween _jump;
        private Vector3 _groundPosition;
        private int _waypoint;
        private RaycastHit[] _hits = new RaycastHit[32];
        private Collider[] _overlaps = new Collider[16];

        public Vector3 GroundPosition => _groundPosition;
        public LevelPath Path => _path;
        public bool IsJumping => _jump != null && _jump.IsActive();
        public bool IsBlocked { get; private set; }
        public Vector3 ForwardDirection
        {
            get
            {
                if (_path == null || !_path.IsValid || _waypoint >= _path.Count) return transform.forward;
                Vector3 direction = _path.GetPoint(_waypoint) - _groundPosition;
                direction.y = 0f;
                return direction.sqrMagnitude > .0001f ? direction.normalized : transform.forward;
            }
        }

        [Inject]
        public void Construct(GameplayConfig config, IGameplayService gameplay, LevelPath path)
        {
            _config = config;
            _gameplay = gameplay;
            _path = path;
            if (_mass == null) _mass = GetComponent<PlayerMassController>();
            _groundPosition = transform.position;
            if (_path == null || !_path.IsValid)
                Debug.LogError("Player requires a LevelPath with ground waypoints. Run Tools/Shrink to Shoot/Gameplay/Set Up Existing Assets and Active Scene.", this);
        }

        private void Awake() => _groundPosition = transform.position;

        private void Update()
        {
            if (_config == null || _gameplay.GameState.CurrentState != GameStates.Play || IsJumping ||
                _path == null || !_path.IsValid || _mass == null) return;

            while (_waypoint < _path.Count && Vector3.Distance(_groundPosition, _path.GetPoint(_waypoint)) < .01f)
                _waypoint++;
            if (_waypoint >= _path.Count)
            {
                IsBlocked = false;
                _gameplay.GameState.ChangeState(GameStates.Win);
                return;
            }

            Vector3 target = _path.GetPoint(_waypoint);
            float segmentLength = Vector3.Distance(_groundPosition, target);
            float jumpDistance = Mathf.Min(_config.JumpDistance, segmentLength);
            // Look far enough to guarantee the landing also retains the configured stand-off distance.
            float hitDistance = FindObstacleDistance(_config.JumpDistance + _config.ObstacleDetectionDistance);
            float safeDistance = float.IsPositiveInfinity(hitDistance) ? jumpDistance :
                Mathf.Max(0f, hitDistance - Mathf.Max(.05f, _config.ObstacleDetectionDistance));
            jumpDistance = Mathf.Min(jumpDistance, safeDistance);
            IsBlocked = jumpDistance < .01f;
            if (IsBlocked) return;

            Vector3 from = _groundPosition;
            Vector3 to = Vector3.MoveTowards(from, target, jumpDistance);
            float duration = Mathf.Max(.05f, _config.JumpDuration * jumpDistance / _config.JumpDistance);
            _jumpFeedback?.PlayFeedbacks();
            _jump = DOVirtual.Float(0f, 1f, duration, progress =>
                {
                    progress = Mathf.Clamp01(progress);
                    _groundPosition = Vector3.LerpUnclamped(from, to, progress);
                    transform.position = _groundPosition + Vector3.up * (Mathf.Sin(progress * Mathf.PI) * _config.JumpHeight);
                })
                .SetEase(_config.JumpEase)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(() =>
                {
                    _groundPosition = to;
                    transform.position = to;
                    _jump = null;
                });
        }

        private float FindObstacleDistance(float horizon)
        {
            Vector3 origin = _groundPosition;
            float travelled = 0f;
            for (int i = _waypoint; i < _path.Count && travelled < horizon; i++)
            {
                Vector3 point = _path.GetPoint(i);
                Vector3 delta = point - origin;
                float length = delta.magnitude;
                if (length < .001f) continue;
                Vector3 direction = delta / length;
                float distance = Mathf.Min(length, horizon - travelled);
                float collision = CastSegment(origin, direction, distance, _mass.CurrentRadius);
                if (!float.IsPositiveInfinity(collision)) return travelled + collision;
                travelled += distance;
                origin = point;
            }
            return float.PositiveInfinity;
        }

        private float CastSegment(Vector3 ground, Vector3 direction, float distance, float radius)
        {
            Vector3 center = ground + Vector3.up * radius;
            int overlapCount;
            while ((overlapCount = Physics.OverlapSphereNonAlloc(center, radius, _overlaps,
                       _config.ObstacleLayerMask, QueryTriggerInteraction.Ignore)) == _overlaps.Length)
                Array.Resize(ref _overlaps, _overlaps.Length * 2);
            for (int i = 0; i < overlapCount; i++)
            {
                Collider collider = _overlaps[i];
                Obstacle obstacle = collider.GetComponentInParent<Obstacle>();
                // A tree behind the player must not prevent forward movement.
                if (obstacle != null && !obstacle.IsDestroying &&
                    Vector3.Dot(collider.bounds.center - center, direction) >= 0f)
                    return 0f;
            }

            int count;
            while ((count = Physics.SphereCastNonAlloc(center, radius, direction, _hits, distance,
                       _config.ObstacleLayerMask, QueryTriggerInteraction.Ignore)) == _hits.Length)
                Array.Resize(ref _hits, _hits.Length * 2);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Obstacle obstacle = _hits[i].collider.GetComponentInParent<Obstacle>();
                if (obstacle == null || obstacle.IsDestroying) continue;
                if (Vector3.Dot(_hits[i].collider.bounds.center - center, direction) < 0f) continue;
                nearest = Mathf.Min(nearest, _hits[i].distance);
            }
            return nearest;
        }

        private void OnDisable()
        {
            _jump?.Kill();
            _jump = null;
            transform.position = _groundPosition;
            IsBlocked = false;
        }
    }
}
