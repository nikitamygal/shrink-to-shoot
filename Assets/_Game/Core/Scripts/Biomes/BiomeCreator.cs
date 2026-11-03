using UnityEngine;
using Game.Configs;
using Game.Tools;
using VContainer;

namespace Game.Biomes
{
    public class BiomeCreator : InjectMonoBehaviour
    {
        [Inject] private readonly IObjectResolver _resolver;

        public void CreateBiome(BiomeConfig biomeConfig)
        {
            if (biomeConfig == null)
            {
                Debug.LogError("BiomesConfig is not assigned.");
                return;
            }
            if (biomeConfig.BiomePrefab == null)
            {
                Debug.LogError($"Biome prefab of type {biomeConfig.BiomeType} is not assigned.");
                return;
            }

            ClearBiome();
            GameObject biome = Instantiate(biomeConfig.BiomePrefab, transform.position + biomeConfig.Offset, Quaternion.identity, transform);
            _resolver?.InjectAndInitialize(biome);
            biome.transform.localScale = biomeConfig.Scale;
        }

        private void ClearBiome()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null)
                    continue;

                child.gameObject.SetActive(false);

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}
