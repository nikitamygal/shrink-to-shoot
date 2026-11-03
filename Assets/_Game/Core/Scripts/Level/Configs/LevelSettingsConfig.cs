using UnityEngine;

namespace Game.Levels
{
    public enum LevelComplexity
    {
        Minimal = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Expert = 4
    }

    [CreateAssetMenu(menuName = "Game/Configs/LevelSettingsConfig", fileName = "LevelSettingsConfig")]
    public class LevelSettingsConfig : ScriptableObject
    {
        public LevelComplexity Complexity = LevelComplexity.Minimal;

        [Header("Configs")]
        public WorldSettingsConfig WorldSettingsConfig;
    }
}
