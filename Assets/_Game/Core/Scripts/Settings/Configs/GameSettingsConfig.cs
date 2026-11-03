using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Game/Configs/GameSettingsConfig", fileName = "GameSettingsConfig")]
    public class GameSettingsConfig : ScriptableObject
    {
        [Header("Audio")]
        [Range(0f, 1f)] public float DefaultMusicVolume = 0.5f;
        [Range(0f, 1f)] public float DefaultSoundVolume = 1f;
        [Min(0f)] public float SoundVolumeMultiplier = 1.3f;

    }
}
