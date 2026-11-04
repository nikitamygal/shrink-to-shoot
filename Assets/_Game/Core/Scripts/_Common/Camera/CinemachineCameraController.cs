using Game.Events;
using Game.Events.Gameplay;
using Game.Tools;
using Game.Levels;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace Game.CameraTools
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineCameraController : InjectMonoBehaviour
    {
        private IEventBus _eventBus;
        private CinemachineCamera _cinemachineCamera;
        private CameraTarget _target;
        private bool _subscribed;
        private ILevelService _levelService;

        [Inject]
        public void Construct(IEventBus eventBus, ILevelService levelService)
        {
            _eventBus = eventBus;
            _levelService = levelService;
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            TrySubscribe();
            SetTarget(_levelService.MainTarget);
        }

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            if (!TryGetComponent<CinemachineFixedHeight>(out _))
                gameObject.AddComponent<CinemachineFixedHeight>();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnChangeCameraPriority(ChangeCameraPriority e)
        {
            if (e.settings != null) _cinemachineCamera.Priority = e.settings.Priority;
        }

        private void OnSetCameraTarget(SetCameraTarget e)
        {
            SetTarget(e.target);
        }

        private void SetTarget(CameraTarget target)
        {
            // Keep the authored startup target until the player has actually spawned.
            if (target == null) return;
            _target = target;
            if (_cinemachineCamera != null)
                _cinemachineCamera.Follow = _target.transform;
        }

        private void TrySubscribe()
        {
            if (_eventBus == null || _subscribed || !isActiveAndEnabled)
                return;

            _eventBus.Subscribe<ChangeCameraPriority>(OnChangeCameraPriority);
            _eventBus.Subscribe<SetCameraTarget>(OnSetCameraTarget);
            _subscribed = true;
        }

        private void TryUnsubscribe()
        {
            if (_eventBus == null || !_subscribed)
                return;

            _eventBus.Unsubscribe<ChangeCameraPriority>(OnChangeCameraPriority);
            _eventBus.Unsubscribe<SetCameraTarget>(OnSetCameraTarget);
            _subscribed = false;
        }

        private void OnEnable()
        {
            TrySubscribe();
            if (_levelService != null) SetTarget(_levelService.MainTarget);
        }

        private void OnDisable()
        {
            TryUnsubscribe();
        }

        private void OnDestroy()
        {
            TryUnsubscribe();
        }
    }
}
