using Game.Configs;
using Game.Core.SaveLoad;
using Game.Sound;
using UnityEngine;
using VContainer.Unity;

namespace Game.Settings
{
    public class SoundLoader : IInitializable, IStartable
    {
        private readonly ISaveLoadService _save;
        private readonly GameSettingsConfig _gameSettingsConfig;
        private readonly ISoundService _sound;

        public SoundLoader(ISaveLoadService save, GameSettingsConfig gameSettingsConfig, ISoundService sound)
        {
            _save = save;
            _gameSettingsConfig = gameSettingsConfig;
            _sound = sound;
        }

        void IInitializable.Initialize()
        {
            ApplySavedVolumes();
        }

        void IStartable.Start()
        {
            ApplySavedVolumes();
        }

        private void ApplySavedVolumes()
        {
            if (_save?.Data == null)
                return;

            if (!_save.Data.HasAudioSettings)
            {
                _save.Data.HasAudioSettings = true;
                _save.Data.MusicVolume = Mathf.Clamp01(_gameSettingsConfig != null ? _gameSettingsConfig.DefaultMusicVolume : 1f);
                _save.Data.SoundVolume = Mathf.Clamp01(_gameSettingsConfig != null ? _gameSettingsConfig.DefaultSoundVolume : 1f);
                _save.SaveData();
            }

            _sound.SetMusicVolume(_save.Data.MusicVolume);
            _sound.SetSoundVolume(_save.Data.SoundVolume);
            _sound.ApplySettings();
        }
    }
}
