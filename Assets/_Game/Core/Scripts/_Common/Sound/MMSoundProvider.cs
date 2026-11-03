using Game.Configs;
using MoreMountains.Tools;
using UnityEngine;

namespace Game.Sound
{
    public class MMSoundProvider : ISoundProvider
    {
        private readonly GameSettingsConfig _gameSettingsConfig;

        public MMSoundProvider(GameSettingsConfig gameSettingsConfig)
        {
            _gameSettingsConfig = gameSettingsConfig;
        }

        public void SetAudioEnabled(bool isEnabled)
        {
            if (!MMSoundManager.HasInstance)
                return;

            // Repeated MuteMaster calls overwrite MMSoundManager's saved master volume.
            if (MMSoundManager.Instance.IsMuted(MMSoundManager.MMSoundManagerTracks.Master) == !isEnabled)
                return;

            if (isEnabled)
                MMSoundManager.Instance.UnmuteMaster();
            else
                MMSoundManager.Instance.MuteMaster();
        }

        public void SetMusicVolume(float volume)
        {
            if (!MMSoundManager.HasInstance)
                return;

            if (volume > 0f)
            {
                MMSoundManager.Instance.UnmuteMusic();
                MMSoundManager.Instance.SetVolumeMusic(volume);
            }
            else
            {
                MMSoundManager.Instance.MuteMusic();
            }
        }

        public void SetSoundVolume(float volume)
        {
            if (!MMSoundManager.HasInstance)
                return;

            if (volume > 0f)
            {
                float multiplier = Mathf.Max(0f, _gameSettingsConfig != null ? _gameSettingsConfig.SoundVolumeMultiplier : 1f);
                float outputVolume = volume * multiplier;
                MMSoundManager.Instance.UnmuteSfx();
                MMSoundManager.Instance.UnmuteUI();
                MMSoundManager.Instance.SetVolumeSfx(outputVolume);
                MMSoundManager.Instance.SetVolumeUI(outputVolume);
            }
            else
            {
                MMSoundManager.Instance.MuteSfx();
                MMSoundManager.Instance.MuteUI();
            }
        }

        public void PlayMusic(AudioClip clip, bool loop, int id)
        {
            if (clip == null)
                return;

            var options = MMSoundManagerPlayOptions.Default;
            options.ID = id;
            options.Loop = loop;
            options.Location = Vector3.zero;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            MMSoundManagerSoundPlayEvent.Trigger(clip, options);
        }
    }
}
