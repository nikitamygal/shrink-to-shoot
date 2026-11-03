using Game.Tools;
using UnityEngine;
using VContainer;

namespace Game.Core.SaveLoad
{
    public class ApplicationSaveHandler : InjectMonoBehaviour
    {
        private ISaveLoadService _save;
        private bool _isQuitting;

        private void OnEnable()
        {
            _isQuitting = false;
            Application.wantsToQuit += OnWantsToQuit;
            Application.quitting += OnApplicationQuitting;
        }

        private void OnDisable()
        {
            Application.wantsToQuit -= OnWantsToQuit;
            Application.quitting -= OnApplicationQuitting;
        }

        [Inject]
        public void Construct(
            ISaveLoadService save)
        {
            _save = save;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveAll();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveAll();
        }

        private bool OnWantsToQuit()
        {
            SaveAll();
            return true;
        }

        // Set this only once quitting is confirmed: another wantsToQuit listener
        // can cancel the request, in which case normal autosaves must continue.
        private void OnApplicationQuitting() => _isQuitting = true;

        private void SaveAll()
        {
            if (!_isQuitting && _save?.IsSaveDataLoaded == true)
                _save.SaveData();
        }
    }
}
