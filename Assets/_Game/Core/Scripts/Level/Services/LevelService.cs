using Game.Biomes;
using Game.CameraTools;
using Game.Core.SaveLoad;
using Game.Events;
using Game.Events.Gameplay;
using VContainer;

namespace Game.Levels
{
    public class LevelService : ILevelService
    {
        public LevelListConfig ListConfig { get; }
        public LevelSettingsConfig LevelConfig { get; private set; }
        public CameraTarget MainTarget => _mainTarget;

        private CameraTarget _mainTarget;

        [Inject] private readonly ISaveLoadService _save;
        [Inject] private readonly BiomeCreator _biomeCreator;
        [Inject] private readonly IEventBus _eventBus;

        public event System.Action OnLevelBuilding;
        public event System.Action OnLevelBuilt;

        public LevelService(LevelListConfig config)
        {
            ListConfig = config;
        }

        public int GetLevelIndex() => _save.Data.LevelIndex;

        public void IncrementLevelIndex()
        {
            _save.Data.LevelIndex++;
            _save.SaveData();
        }

        public void BuildLevel()
        {
            OnLevelBuilding?.Invoke();
            LevelConfig = ListConfig.GetLevelSettings(GetLevelIndex());
            if (_biomeCreator != null && LevelConfig?.WorldSettingsConfig?.BiomeConfig != null)
                _biomeCreator.CreateBiome(LevelConfig.WorldSettingsConfig.BiomeConfig);
            OnLevelBuilt?.Invoke();
        }

        public void RegisterMainTarget(CameraTarget target)
        {
            if (target == null)
                return;

            _mainTarget = target;
            _eventBus?.Trigger(new SetCameraTarget(_mainTarget));
        }
    }
}
