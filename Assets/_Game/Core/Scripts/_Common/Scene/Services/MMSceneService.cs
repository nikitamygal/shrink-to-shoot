using Cysharp.Threading.Tasks;
using Game.Core.Constants;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Services
{
    /// <summary>
    /// Scene loading service based on MMAdditiveSceneLoadingManager.
    /// Supports fades, progress interpolation, and MM flow.
    /// </summary>
    public class MMSceneService : SceneService
    {
        private readonly MMAdditiveSceneLoadingManagerSettings _settings = new MMAdditiveSceneLoadingManagerSettings();

        public MMSceneService()
        {
            // Use the Loading scene for all transitions.
            _settings.LoadingSceneName = Scenes.Loading;
            _settings.AntiSpillSceneName = "";
            _settings.InterpolateProgress = true;
            // A full bar takes 0.8 seconds, keeping fast loads visible.
            _settings.ProgressBarSpeed = 1.25f;

            _settings.BeforeEntryFadeDelay = 0f;
            _settings.EntryFadeDuration = 0.25f; // Reveal the loading screen from black.
            _settings.AfterEntryFadeDelay = 0.05f;

            _settings.ExitFadeDuration = 0.25f; // Fade to black before removing the screen.
            _settings.BeforeSceneActivationDelay = 0f;
            _settings.AfterSceneActivationDelay = 0f;
            _settings.FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeOutThenIn;
        }

        public override async UniTask LoadSceneAsync(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"Scene load already in progress, ignoring {sceneName}");
                return;
            }

            _isLoading = true;

            var previousScene = SceneManager.GetActiveScene();
            MMAdditiveSceneLoadingManager.LoadScene(sceneName, _settings);

            await UniTask.WaitUntil(() =>
            {
                var active = SceneManager.GetActiveScene();
                return active.IsValid() && active != previousScene && active.name == sceneName &&
                    !SceneManager.GetSceneByName(Scenes.Loading).isLoaded;
            });

            _isLoading = false;
        }
    }
}
