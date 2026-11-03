using Game.Configs;
using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Game/Configs/WorldSettingsConfig", fileName = "WorldSettingsConfig")]
    public class WorldSettingsConfig : ScriptableObject
    {
        [Header("Configs")]
        public BiomeConfig BiomeConfig;

    }
}
