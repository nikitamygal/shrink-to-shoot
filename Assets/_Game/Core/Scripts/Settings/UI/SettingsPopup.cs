using System;
using Game.Configs;
using Game.Core.SaveLoad;
using Game.Tools;
using Game.UI;
using Game.UI.PopupWindow;
using Game.Sound;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Game.Settings
{
    public class SettingsPopup : UIPanel, IPopupContent, IInitializable, IDisposable
    {
        [Header("Music")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Image _musicIcon;
        [SerializeField] private Sprite _musicOnImage;
        [SerializeField] private Sprite _musicOffImage;

        [Header("Sound")]
        [SerializeField] private Slider _soundSlider;
        [SerializeField] private Image _soundIcon;
        [SerializeField] private Sprite _soundOnImage;
        [SerializeField] private Sprite _soundOffImage;

        [Inject] private readonly ISaveLoadService _save;
        [Inject] private readonly GameSettingsConfig _gameSettingsConfig;
        [Inject] private readonly ISoundService _sound;

        public string PopupTitle { get; } = "Settings";

        #region PANEL SHOW/HIDE
        public override void Show()
        {
            base.Show();
            Timescale.Pause();
        }

        public override void Hide()
        {
            base.Hide();
            Timescale.UnPause();
        }
        #endregion

        private void OnMusicSliderChanged(float value)
        {
            ApplyMusic(value);
            SaveAudioValues();
        }

        private void OnSoundSliderChanged(float value)
        {
            ApplySound(value);
            SaveAudioValues();
        }

        private void BindSliders()
        {
            if (_musicSlider != null)
            {
                _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
                _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            }

            if (_soundSlider != null)
            {
                _soundSlider.onValueChanged.RemoveListener(OnSoundSliderChanged);
                _soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
            }
        }

        private void UnbindSliders()
        {
            if (_musicSlider != null)
                _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

            if (_soundSlider != null)
                _soundSlider.onValueChanged.RemoveListener(OnSoundSliderChanged);
        }

        private void InitializeAudio()
        {
            if (_save?.Data == null)
                return;

            InitializeAudioSettingsIfNeeded();

            float musicVolume = ClampSliderValue(_save.Data.MusicVolume);
            float soundVolume = ClampSliderValue(_save.Data.SoundVolume);

            if (_musicSlider != null)
                _musicSlider.SetValueWithoutNotify(musicVolume);

            if (_soundSlider != null)
                _soundSlider.SetValueWithoutNotify(soundVolume);

            ApplyMusic(musicVolume);
            ApplySound(soundVolume);
        }

        private void InitializeAudioSettingsIfNeeded()
        {
            if (_save?.Data == null || _save.Data.HasAudioSettings)
                return;

            _save.Data.HasAudioSettings = true;
            _save.Data.MusicVolume = ClampSliderValue(_gameSettingsConfig != null ? _gameSettingsConfig.DefaultMusicVolume : 1f);
            _save.Data.SoundVolume = ClampSliderValue(_gameSettingsConfig != null ? _gameSettingsConfig.DefaultSoundVolume : 1f);
            _save.SaveData();
        }

        private void ApplyMusic(float value)
        {
            float clampedValue = ClampSliderValue(value);
            bool isEnabled = clampedValue > 0f;

            if (_musicIcon != null)
                _musicIcon.sprite = isEnabled ? _musicOnImage : _musicOffImage;

            _sound.SetMusicVolume(clampedValue);
        }

        private void ApplySound(float value)
        {
            float clampedValue = ClampSliderValue(value);
            bool isEnabled = clampedValue > 0f;

            if (_soundIcon != null)
                _soundIcon.sprite = isEnabled ? _soundOnImage : _soundOffImage;

            _sound.SetSoundVolume(clampedValue);
        }

        private void SaveAudioValues()
        {
            if (_save?.Data == null)
                return;

            _save.Data.HasAudioSettings = true;
            _save.Data.MusicVolume = _sound.MusicVolume;
            _save.Data.SoundVolume = _sound.SoundVolume;
            _save.SaveData();
        }

        private static float ClampSliderValue(float value)
        {
            return Mathf.Clamp01(value);
        }

        void IInitializable.Initialize()
        {
            BindSliders();
            InitializeAudio();
        }

        void IDisposable.Dispose()
        {
            UnbindSliders();
        }
    }
}
