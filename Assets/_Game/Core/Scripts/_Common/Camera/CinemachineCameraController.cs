using Game.Events;
using Game.Events.Gameplay;
using Game.Tools;
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

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            TrySubscribe();
        }

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnChangeCameraPriority(ChangeCameraPriority e)
        {
            _cinemachineCamera.Priority = e.settings.Priority;
        }

        private void OnSetCameraTarget(SetCameraTarget e)
        {
            _cinemachineCamera.Follow = _target.transform;
        }

        private void TrySubscribe()
        {
            if (_eventBus == null || _subscribed)
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
        }

        private void OnDisable()
        {
            TryUnsubscribe();
        }
    }
}
