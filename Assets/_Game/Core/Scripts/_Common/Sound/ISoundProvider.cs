using UnityEngine;

namespace Game.Sound
{
    public interface ISoundProvider
    {
        void SetAudioEnabled(bool isEnabled);
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);
        void PlayMusic(AudioClip clip, bool loop, int id);
    }
}
