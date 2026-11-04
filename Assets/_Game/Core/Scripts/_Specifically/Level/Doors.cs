using DG.Tweening;
using Game.Configs;
using Game.Gameplay;
using Game.Levels;
using Game.Tools;
using UnityEngine;
using VContainer;

namespace Game.Core.Controllers
{
    [DisallowMultipleComponent]
    public sealed class Doors : InjectMonoBehaviour
    {
        [SerializeField] private Transform _leftPivot;
        [SerializeField] private Transform _rightPivot;

        private GameplayConfig _config;
        private ILevelService _level;
        private IGameplayService _gameplay;
        private Sequence _opening;
        private bool _opened;

        [Inject]
        public void Construct(GameplayConfig config, ILevelService level, IGameplayService gameplay)
        {
            _config = config;
            _level = level;
            _gameplay = gameplay;
        }

        private void Update()
        {
            if (_opened || _config == null || _gameplay.GameState.CurrentState != GameStates.Play ||
                _level.MainTarget == null || _leftPivot == null || _rightPivot == null) return;

            if ((_level.MainTarget.transform.position - transform.position).sqrMagnitude >=
                _config.DoorOpeningDistance * _config.DoorOpeningDistance) return;

            _opened = true;
            float duration = Mathf.Max(.01f, _config.DoorOpeningDuration);
            _opening = DOTween.Sequence()
                .Join(_leftPivot.DOLocalRotate(_leftPivot.localEulerAngles + Vector3.up * -_config.DoorOpeningAngle,
                    duration).SetEase(Ease.OutCubic))
                .Join(_rightPivot.DOLocalRotate(_rightPivot.localEulerAngles + Vector3.up * _config.DoorOpeningAngle,
                    duration).SetEase(Ease.OutCubic))
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void OnDisable()
        {
            _opening?.Kill();
            _opening = null;
        }
    }
}
