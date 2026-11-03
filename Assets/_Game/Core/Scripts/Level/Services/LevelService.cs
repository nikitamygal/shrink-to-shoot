using Game.Biomes;
using Game.Core.SaveLoad;
using VContainer;

namespace Game.Levels
{
    public class LevelService : ILevelService
    {
        public LevelListConfig ListConfig { get; }
        public LevelSettingsConfig LevelConfig { get; private set; }

        [Inject] private readonly ISaveLoadService _save;
        [Inject] private readonly BiomeCreator _biomeCreator;

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
    }
}
