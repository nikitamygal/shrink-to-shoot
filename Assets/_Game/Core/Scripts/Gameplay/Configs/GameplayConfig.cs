using AYellowpaper.SerializedCollections;
using Game.Core.Loadouts;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Configs/GameplayConfig", fileName = "GameplayConfig")]
    public class GameplayConfig : ScriptableObject
    {
        [Header("Delays")]
        public float WinDelay = 2.5f;
        public float LoseDelay = 2.0f;

        [Header("Default Loadout")]
        public SerializedDictionary<LoadoutSlotType, ItemSettingsConfig> DefaultLoadout = new();
    }
}
