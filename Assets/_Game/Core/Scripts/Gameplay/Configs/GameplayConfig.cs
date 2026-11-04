using AYellowpaper.SerializedCollections;
using Game.Core.Loadouts;
using UnityEngine;
using DG.Tweening;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Configs/GameplayConfig", fileName = "GameplayConfig")]
    public class GameplayConfig : ScriptableObject
    {
        [Header("Delays")]
        public float WinDelay = 2.5f;
        public float LoseDelay = 2.0f;

        [Header("Default Loadout")]
        public SerializedDictionary<LoadoutSlotType, ItemSettingsConfig> DefaultLoadout = new();

        [Header("Player Movement")]
        [Min(0.01f)] public float JumpDistance = 2f;
        [Min(0f)] public float JumpHeight = 0.65f;
        [Min(0.01f)] public float JumpDuration = 0.5f;
        public Ease JumpEase = Ease.OutQuad;
        [Min(0f), Tooltip("Look-ahead distance in metres, measured from the sphere surface.")]
        public float ObstacleDetectionDistance = 2.5f;
        [Tooltip("Only colliders on these layers block movement and receive explosion damage.")]
        public LayerMask ObstacleLayerMask;

        [Header("Level Finish")]
        [Min(0f)] public float DoorOpeningDistance = 10f;
        [Min(0.01f)] public float DoorOpeningDuration = 0.7f;
        public float DoorOpeningAngle = 90f;
        [Min(0.01f)] public float ExitDuration = 1f;
        public float ExitSpinDegrees = 720f;

        [Header("Player Mass (uniform density, sphere volume)")]
        [Min(0.01f)] public float InitialPlayerRadius = 0.75f;
        [Min(0.01f)] public float MinimumPlayerRadius = 0.2f;
        [Min(0.01f)] public float MinimumProjectileRadius = 0.12f;

        [Header("Projectile")]
        [Min(0.001f), Tooltip("Sphere volume transferred per second, independent of frame rate.")]
        public float ProjectileChargeSpeed = 0.35f;
        [Min(0.01f)] public float ProjectileSpeed = 12f;
        [Min(0f)] public float ProjectileArcHeight = 1.5f;
        [Min(0.01f)] public float ProjectileLifetime = 5f;
        [Min(0f)] public float ProjectileExplosionRadius = 0.5f;
        [Min(0f), Tooltip("Explosion radius = base radius + charged sphere radius × multiplier.")]
        public float ProjectileRadiusMultiplier = 3f;

        [Header("Obstacles")]
        [Min(1)] public int DefaultObstacleHealth = 1;

        private void OnValidate()
        {
            WinDelay = Mathf.Max(0f, WinDelay);
            LoseDelay = Mathf.Max(0f, LoseDelay);
            JumpDistance = Mathf.Max(0.01f, JumpDistance);
            JumpHeight = Mathf.Max(0f, JumpHeight);
            JumpDuration = Mathf.Max(0.01f, JumpDuration);
            ObstacleDetectionDistance = Mathf.Max(0f, ObstacleDetectionDistance);
            DoorOpeningDistance = Mathf.Max(0f, DoorOpeningDistance);
            DoorOpeningDuration = Mathf.Max(0.01f, DoorOpeningDuration);
            ExitDuration = Mathf.Max(0.01f, ExitDuration);
            InitialPlayerRadius = Mathf.Max(0.02f, InitialPlayerRadius);
            MinimumPlayerRadius = Mathf.Clamp(MinimumPlayerRadius, 0.01f, InitialPlayerRadius - 0.001f);
            MinimumProjectileRadius = Mathf.Max(0.01f, MinimumProjectileRadius);
            ProjectileChargeSpeed = Mathf.Max(0.001f, ProjectileChargeSpeed);
            ProjectileSpeed = Mathf.Max(0.01f, ProjectileSpeed);
            ProjectileArcHeight = Mathf.Max(0f, ProjectileArcHeight);
            ProjectileLifetime = Mathf.Max(0.01f, ProjectileLifetime);
            ProjectileExplosionRadius = Mathf.Max(0f, ProjectileExplosionRadius);
            ProjectileRadiusMultiplier = Mathf.Max(0f, ProjectileRadiusMultiplier);
            DefaultObstacleHealth = Mathf.Max(1, DefaultObstacleHealth);
        }
    }
}
