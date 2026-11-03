using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game.Services
{
    public class SceneService : ISceneService
    {
        protected bool _isLoading = false;

        public virtual void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public virtual async UniTask LoadSceneAsync(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
                await UniTask.Yield();
        }

        public void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadScene(currentSceneName);
        }
    }
}
