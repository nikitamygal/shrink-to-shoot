using AYellowpaper.SerializedCollections;
using Game.Core.Enums;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Configs
{   
    [CreateAssetMenu(menuName = "Game/Configs/ItemSettingsConfig", fileName = "ItemSettingsConfig")]
    public class ItemSettingsConfig : ScriptableObject
    {
        public string UniqueKey => _uniqueKey;
        [SerializeField] private string _uniqueKey = string.Empty;

        [Space]
        public string Name;
        [TextArea(5, -1)] public string Description;
        [PreviewField]
        public Sprite Icon;
        public RarityType Rarity;
        public ItemTag Tag;
        public ItemType Type;
        public GameObject Prefab;
        [Tooltip("Parameters that are visible in tooltips")]
        public List<ParameterType> DisplayParameters;
        public SerializedDictionary<ParameterType, float> Parameters;

        public float? GetParameterValue(ParameterType parameterType)
        {
            if (Parameters != null && Parameters.TryGetValue(parameterType, out float value))
            {
                return value;
            }

            return null;
        }
    }
}
