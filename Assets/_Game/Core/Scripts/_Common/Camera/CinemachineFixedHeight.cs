using Unity.Cinemachine;
using UnityEngine;

namespace Game.CameraTools
{
    /// <summary>Keep the flat level's authored camera altitude while Cinemachine follows on X/Z.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    [AddComponentMenu("Cinemachine/Extensions/Fixed World Height")]
    public sealed class CinemachineFixedHeight : CinemachineExtension
    {
        private float _worldHeight;
        private bool _hasHeight;

        protected override void Awake()
        {
            base.Awake();
            _worldHeight = transform.position.y;
            _hasHeight = true;

            var follow = GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                // Follow damps in its tilted offset space. Uniform damping keeps target Y motion
                // from leaking into camera X/Z before the world-height constraint is applied.
                var tracker = follow.TrackerSettings;
                float damping = Mathf.Max(tracker.PositionDamping.x, tracker.PositionDamping.z);
                tracker.PositionDamping = Vector3.one * damping;
                follow.TrackerSettings = tracker;
            }
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (!Application.isPlaying || stage != CinemachineCore.Stage.Body || vcam.Follow == null) return;
            if (!_hasHeight)
            {
                _worldHeight = vcam.transform.position.y;
                _hasHeight = true;
            }
            Vector3 position = state.RawPosition;
            position.y = _worldHeight;
            state.RawPosition = position;
            // Noise and intentional Feel/impulse effects run after Body and remain available.
        }

        public override void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
        {
            base.ForceCameraPosition(vcam, pos, rot);
            _worldHeight = pos.y;
            _hasHeight = true;
        }
    }
}
