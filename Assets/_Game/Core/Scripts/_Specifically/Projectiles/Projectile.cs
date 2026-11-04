using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Configs;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private SphereCollider _sphereCollider;
        [SerializeField] private MMF_Player _impactFeedback;
        [SerializeField, Min(.5f), Tooltip("Safety limit for a feedback accidentally configured to loop forever.")]
        private float _feedbackTimeout = 10f;

        private GameplayConfig _config;
        private Collider[] _overlaps = new Collider[32];
        private RaycastHit[] _hits = new RaycastHit[32];
        private readonly HashSet<Obstacle> _damaged = new HashSet<Obstacle>();
        private Vector3 _start;
        private Vector3 _destination;
        private float _duration;
        private float _elapsed;
        private float _effectElapsed;
        private bool _flying;
        private bool _exploding;
        private bool _finished;
        private Tween _explosionTween;
        private Vector3 _authoredVisualScale;
        private float _authoredSphereRadius;

        public event Action<Projectile> Finished;
        public float Mass { get; private set; }
        public float Radius { get; private set; }
        public float ExplosionRadius => _config == null ? 0f :
            _config.ProjectileExplosionRadius + Radius * _config.ProjectileRadiusMultiplier;

        public bool Initialize(GameplayConfig config)
        {
            _config = config;
            if (_sphereCollider == null) _sphereCollider = GetComponentInChildren<SphereCollider>(true);
            if (_visual == null && _sphereCollider != null) _visual = _sphereCollider.transform;
            if (_visual == null || _sphereCollider == null)
            {
                Debug.LogError("Projectile needs its sphere visual and SphereCollider assigned before charging.", this);
                return false;
            }
            if (_visual == transform)
            {
                Debug.LogError("Assign the child Sphere or Model as Projectile visual; the flight root must stay active while charging.", this);
                return false;
            }
            if (_visual != _sphereCollider.transform && !_sphereCollider.transform.IsChildOf(_visual))
            {
                Debug.LogError("Projectile visual must be its Sphere transform or a parent Model containing that sphere.", this);
                return false;
            }
            // Preserve the authored shape and hierarchy scale. Queries use the true world sphere radius,
            // including a Model wrapper and any scale between that wrapper and the SphereCollider.
            _authoredVisualScale = _visual.localScale;
            Vector3 sphereScale = _sphereCollider.transform.lossyScale;
            _authoredSphereRadius = _sphereCollider.radius *
                Mathf.Max(Mathf.Abs(sphereScale.x), Mathf.Abs(sphereScale.y), Mathf.Abs(sphereScale.z));
            if (!(_authoredSphereRadius > 0f) || float.IsInfinity(_authoredSphereRadius))
            {
                Debug.LogError("Projectile sphere needs a positive collider radius and nonzero visual scale.", this);
                return false;
            }
            // Flight uses swept queries: neither the held preview nor its shell may push the player.
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            CenterVisual();
            return true;
        }

        public void SetMass(float mass)
        {
            if (_flying || _exploding || _finished) return;
            Radius = SphereMass.ToRadius(mass);
            Mass = mass;
            if (_visual == null) return;
            SetVisualRadius(Mathf.Max(.0005f, Radius));
            _visual.gameObject.SetActive(mass > 0f);
        }

        private void SetVisualRadius(float radius)
        {
            if (_visual == null || _authoredSphereRadius <= 0f) return;
            _visual.localScale = _authoredVisualScale * (radius / _authoredSphereRadius);
            CenterVisual();
        }

        private void CenterVisual()
        {
            if (_visual == null || _sphereCollider == null || _visual == transform) return;
            Vector3 sphereCenter = _sphereCollider.transform.TransformPoint(_sphereCollider.center);
            // Move only the selected visual subtree: keep its children's authored offsets intact.
            _visual.position += transform.position - sphereCenter;
        }

        public void Launch(Vector3 destination, Vector3 emissionOrigin)
        {
            if (_config == null || Mass <= 0f || _flying || _exploding || _finished) return;
            _start = transform.position;
            _destination = destination;
            _duration = Mathf.Max(.01f, Vector3.Distance(_start, destination) / _config.ProjectileSpeed);
            _elapsed = 0f;
            _flying = true;
            // A large held preview may extend past a nearby thin tree. Sweep its separation too,
            // so the first obstacle cannot be skipped merely by growing the shot beyond it.
            if (FindOverlap(emissionOrigin, out Obstacle obstacle))
            {
                transform.position = emissionOrigin;
                Explode(obstacle);
                return;
            }
            Vector3 separation = transform.position - emissionOrigin;
            float separationDistance = separation.magnitude;
            if (separationDistance > .00001f && FindImpact(emissionOrigin, separation / separationDistance,
                    separationDistance, out RaycastHit hit, out obstacle))
            {
                transform.position = emissionOrigin + separation.normalized * hit.distance;
                Explode(obstacle);
                return;
            }
            if (FindOverlap(transform.position, out obstacle)) Explode(obstacle);
        }

        private void Update()
        {
            if (_finished || _config == null) return;
            if (_exploding)
            {
                _effectElapsed += Time.deltaTime;
                bool animationPlaying = _explosionTween != null && _explosionTween.IsActive();
                if ((_effectElapsed >= .35f && !animationPlaying && (_impactFeedback == null || !_impactFeedback.IsPlaying)) ||
                    _effectElapsed >= Mathf.Max(.5f, _feedbackTimeout)) Finish();
                return;
            }
            if (!_flying) return;

            // Subdivide hitches so the swept chord still approximates the curved trajectory closely.
            float remaining = Mathf.Min(Time.deltaTime, Mathf.Min(_duration, _config.ProjectileLifetime) - _elapsed);
            while (remaining > .00001f && _flying)
            {
                float step = Mathf.Min(remaining, .02f);
                _elapsed += step;
                remaining -= step;
                float t = Mathf.Clamp01(_elapsed / _duration);
                Vector3 next = Vector3.LerpUnclamped(_start, _destination, t) +
                               Vector3.up * (4f * _config.ProjectileArcHeight * t * (1f - t));
                Vector3 movement = next - transform.position;
                float length = movement.magnitude;
                if (length > .00001f)
                {
                    Vector3 direction = movement / length;
                    if (FindImpact(transform.position, direction, length, out RaycastHit hit, out Obstacle obstacle))
                    {
                        transform.position += direction * hit.distance;
                        Explode(obstacle);
                        break;
                    }
                    transform.rotation = Quaternion.LookRotation(direction);
                }
                transform.position = next;
            }
            if (_flying && (_elapsed >= _duration - .00001f || _elapsed >= _config.ProjectileLifetime - .00001f))
                Finish();
        }

        private bool FindOverlap(Vector3 center, out Obstacle obstacle)
        {
            int count = Overlap(center, Radius);
            obstacle = null;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider collider = _overlaps[i];
                Obstacle candidate = collider.GetComponentInParent<Obstacle>();
                if (candidate == null || candidate.IsDestroying) continue;
                float distance = (collider.ClosestPoint(center) - center).sqrMagnitude;
                if (distance >= nearest) continue;
                nearest = distance;
                obstacle = candidate;
            }
            return obstacle != null;
        }

        private bool FindImpact(Vector3 origin, Vector3 direction, float distance, out RaycastHit firstHit, out Obstacle obstacle)
        {
            int count;
            while ((count = Physics.SphereCastNonAlloc(origin, Radius, direction, _hits, distance,
                       _config.ObstacleLayerMask, QueryTriggerInteraction.Ignore)) == _hits.Length)
                Array.Resize(ref _hits, _hits.Length * 2);
            firstHit = default;
            obstacle = null;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Obstacle candidate = _hits[i].collider.GetComponentInParent<Obstacle>();
                if (candidate == null || candidate.IsDestroying || _hits[i].distance >= nearest) continue;
                nearest = _hits[i].distance;
                firstHit = _hits[i];
                obstacle = candidate;
            }
            return obstacle != null;
        }

        private int Overlap(Vector3 center, float radius)
        {
            int count;
            // Grow only when needed; never silently omit trees when a dense cluster saturates the buffer.
            while ((count = Physics.OverlapSphereNonAlloc(center, radius, _overlaps,
                       _config.ObstacleLayerMask, QueryTriggerInteraction.Ignore)) == _overlaps.Length)
                Array.Resize(ref _overlaps, _overlaps.Length * 2);
            return count;
        }

        private void Explode(Obstacle firstObstacle)
        {
            if (!_flying || _exploding || _finished) return;
            _flying = false;
            _exploding = true;
            _effectElapsed = 0f;
            _damaged.Clear();
            // Snapshot the colliders before any damage disables them.
            int count = Overlap(transform.position, ExplosionRadius);
            if (firstObstacle != null)
            {
                _damaged.Add(firstObstacle);
                firstObstacle.TakeDamage(1);
            }
            for (int i = 0; i < count; i++)
            {
                Obstacle obstacle = _overlaps[i].GetComponentInParent<Obstacle>();
                if (obstacle != null && !obstacle.IsDestroying && _damaged.Add(obstacle)) obstacle.TakeDamage(1);
            }
            _impactFeedback?.PlayFeedbacks();
            if (_visual != null)
            {
                _explosionTween = DOTween.Sequence()
                    .Append(DOVirtual.Float(Radius, ExplosionRadius, .12f, SetVisualRadius).SetEase(Ease.OutQuad))
                    .Append(DOVirtual.Float(ExplosionRadius, .0005f, .2f, SetVisualRadius).SetEase(Ease.InQuad))
                    .SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }
        }

        public void Cancel() => Finish();

        private void Finish()
        {
            if (_finished) return;
            _finished = true;
            _flying = _exploding = false;
            _explosionTween?.Kill();
            _explosionTween = null;
            Finished?.Invoke(this);
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            _explosionTween?.Kill();
            _explosionTween = null;
            _impactFeedback?.StopFeedbacks();
            if (_finished) return;
            _finished = true;
            _flying = _exploding = false;
            Finished?.Invoke(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;
            Gizmos.color = new Color(1f, .5f, 0f);
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
        }
    }
}
