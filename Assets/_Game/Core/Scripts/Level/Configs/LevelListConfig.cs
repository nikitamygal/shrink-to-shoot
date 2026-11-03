using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Game/Configs/LevelListConfig", fileName = "LevelListConfig")]
    public class LevelListConfig : ScriptableObject
    {
        [Tooltip("Full ordered list of levels")]
        public List<LevelSettingsConfig> Levels = new();

        public LevelSettingsConfig GetLevelSettings(int globalLevelIndex)
        {
            if (Levels == null || Levels.Count == 0)
                return null;

            return Levels[globalLevelIndex % Levels.Count];
        }
    }
}
