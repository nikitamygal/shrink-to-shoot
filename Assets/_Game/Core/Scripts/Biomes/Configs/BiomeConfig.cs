using UnityEngine;
using Game.Core.Enums;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Configs/BiomeConfig", fileName = "BiomeConfig")]
    public class BiomeConfig : ScriptableObject
    {
        public BiomeType BiomeType;
        public GameObject BiomePrefab;
        public Vector3 Offset;
        public Vector3 Scale = Vector3.one;
    }
}
