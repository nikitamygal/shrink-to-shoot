using UnityEngine;

namespace Game.Sound
{
    public interface ISoundService
    {
        float MusicVolume { get; }
        float SoundVolume { get; }
        bool IsAudioEnabled { get; }
        bool IsMusicEnabled { get; }
        bool IsSoundEnabled { get; }

        void SetAudioEnabled(bool isEnabled);
        void SetMusicEnabled(bool isEnabled);
        void SetSoundEnabled(bool isEnabled);
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);
        void ApplySettings();
        void PlayMusic(AudioClip clip, bool loop = true, int id = 255);
    }
}
