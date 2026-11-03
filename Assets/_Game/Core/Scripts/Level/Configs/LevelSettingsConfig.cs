using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Game/Configs/LevelSettingsConfig", fileName = "LevelSettingsConfig")]
    public class LevelSettingsConfig : ScriptableObject
    {
        [Header("Configs")]
        public WorldSettingsConfig WorldSettingsConfig;
    }
}
