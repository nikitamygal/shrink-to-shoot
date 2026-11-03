using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Configs/BiomeListConfig", fileName = "BiomeListConfig")]
    public class BiomeListConfig : ScriptableObject
    {
        public BiomeConfig[] List;

        public BiomeConfig GetBiome(int level)
        {
            if (List == null || List.Length == 0)
            {
                Debug.LogError("Biome list is empty. Please assign biomes in BiomeListConfig.");
                return null;
            }

            int biomeIndex = level % List.Length;
            return List[biomeIndex];
        }
    }
}
