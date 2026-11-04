using UnityEngine;

namespace Game.Core.Controllers
{
    /// <summary>Ground-plane waypoints; the final waypoint is also the fixed shooting goal.</summary>
    [DisallowMultipleComponent]
    public sealed class LevelPath : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints = new Transform[0];
        public int Count => _waypoints?.Length ?? 0;
        public bool IsValid
        {
            get
            {
                if (Count == 0) return false;
                for (int i = 0; i < Count; i++) if (_waypoints[i] == null) return false;
                return true;
            }
        }
        public Vector3 GoalPosition => Count > 0 && _waypoints[Count - 1] != null
            ? _waypoints[Count - 1].position : transform.position;
        public Vector3 GetPoint(int index) => _waypoints[index].position;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < Count; i++)
            {
                if (_waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(_waypoints[i].position, .15f);
                if (i > 0 && _waypoints[i - 1] != null)
                    Gizmos.DrawLine(_waypoints[i - 1].position, _waypoints[i].position);
            }
        }
    }
}
