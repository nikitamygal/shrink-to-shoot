using System;

namespace Game.Core.SaveLoad
{
    [Serializable]
    public class SaveData
    {
        public int LevelIndex = 0;

        // Settings
        public bool HasAudioSettings = false;
        public float MusicVolume = 1f;
        public float SoundVolume = 1f;
    }
}
