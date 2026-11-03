using System;

namespace Game.Levels
{
    public interface ILevelService
    {
        LevelListConfig ListConfig { get; }
        LevelSettingsConfig LevelConfig { get; }
        event Action OnLevelBuilding;
        event Action OnLevelBuilt;

        int GetLevelIndex();
        void IncrementLevelIndex();
        void BuildLevel();
    }
}
