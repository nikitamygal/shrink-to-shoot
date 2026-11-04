using System;

namespace Game.Core.Controllers
{
    /// <summary>Unit-density sphere volume is the resource shared by player and projectile.</summary>
    public static class SphereMass
    {
        public static float FromRadius(float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            double volume = (4d / 3d) * Math.PI * radius * radius * radius;
            if (volume > float.MaxValue) throw new ArgumentOutOfRangeException(nameof(radius));
            return (float)volume;
        }

        public static float ToRadius(float mass)
        {
            if (float.IsNaN(mass) || float.IsInfinity(mass) || mass < 0f)
                throw new ArgumentOutOfRangeException(nameof(mass));
            return (float)Math.Pow(mass * 3d / (4d * Math.PI), 1d / 3d);
        }
    }

    /// <summary>Transactional transfer: preview removes volume, cancel restores it, commit keeps it spent.</summary>
    public sealed class SphereMassTransfer
    {
        public float InitialMass { get; }
        public float MinimumMass { get; }
        public float CurrentMass { get; private set; }
        public float PendingMass { get; private set; }
        public bool IsTransferring { get; private set; }
        public bool IsCritical => CurrentMass <= MinimumMass;
        private float _transferStartMass;

        public SphereMassTransfer(float initialRadius, float minimumRadius)
        {
            if (minimumRadius <= 0f || initialRadius <= minimumRadius)
                throw new ArgumentOutOfRangeException(nameof(minimumRadius));
            InitialMass = CurrentMass = SphereMass.FromRadius(initialRadius);
            MinimumMass = SphereMass.FromRadius(minimumRadius);
        }

        public bool Begin()
        {
            if (IsTransferring || IsCritical) return false;
            _transferStartMass = CurrentMass;
            PendingMass = 0f;
            IsTransferring = true;
            return true;
        }

        public float Transfer(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (!IsTransferring) return 0f;
            float nextMass = Math.Max(MinimumMass, CurrentMass - amount);
            float transferred = CurrentMass - nextMass;
            CurrentMass = nextMass;
            PendingMass = _transferStartMass - CurrentMass;
            return transferred;
        }

        public float Commit()
        {
            if (!IsTransferring) return 0f;
            float committed = PendingMass;
            PendingMass = 0f;
            IsTransferring = false;
            return committed;
        }

        public void Cancel()
        {
            if (!IsTransferring) return;
            CurrentMass = _transferStartMass;
            PendingMass = 0f;
            IsTransferring = false;
        }
    }
}
