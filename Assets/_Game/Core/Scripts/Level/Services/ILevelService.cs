using System;
using Game.CameraTools;

namespace Game.Levels
{
    public interface ILevelService
    {
        LevelListConfig ListConfig { get; }
        LevelSettingsConfig LevelConfig { get; }
        CameraTarget MainTarget { get; }

        event Action OnLevelBuilding;
        event Action OnLevelBuilt;

        int GetLevelIndex();
        void IncrementLevelIndex();
        void BuildLevel();
        void RegisterMainTarget(CameraTarget target);
    }
}
