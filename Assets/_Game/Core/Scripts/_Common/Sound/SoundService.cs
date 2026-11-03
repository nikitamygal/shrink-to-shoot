using UnityEngine;

namespace Game.Sound
{
    public class SoundService : ISoundService
    {
        private readonly ISoundProvider _provider;

        public float MusicVolume { get; private set; } = 1f;
        public float SoundVolume { get; private set; } = 1f;
        public bool IsAudioEnabled { get; private set; } = true;
        public bool IsMusicEnabled { get; private set; } = true;
        public bool IsSoundEnabled { get; private set; } = true;

        public SoundService(ISoundProvider provider)
        {
            _provider = provider;
        }

        // Global muting preserves the player's music and sound settings.
        public void SetAudioEnabled(bool isEnabled)
        {
            IsAudioEnabled = isEnabled;
            _provider.SetAudioEnabled(isEnabled);
        }

        public void SetMusicEnabled(bool isEnabled)
        {
            IsMusicEnabled = isEnabled;
            ApplyMusic();
        }

        public void SetSoundEnabled(bool isEnabled)
        {
            IsSoundEnabled = isEnabled;
            ApplySound();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            ApplyMusic();
        }

        public void SetSoundVolume(float volume)
        {
            SoundVolume = Mathf.Clamp01(volume);
            ApplySound();
        }

        public void ApplySettings()
        {
            _provider.SetAudioEnabled(IsAudioEnabled);
            ApplyMusic();
            ApplySound();
        }

        public void PlayMusic(AudioClip clip, bool loop = true, int id = 255)
        {
            ApplySettings();
            _provider.PlayMusic(clip, loop, id);
        }

        private void ApplyMusic()
        {
            _provider.SetMusicVolume(IsMusicEnabled ? MusicVolume : 0f);
        }

        private void ApplySound()
        {
            _provider.SetSoundVolume(IsSoundEnabled ? SoundVolume : 0f);
        }
    }
}
