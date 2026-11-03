using Cysharp.Threading.Tasks;

namespace Game.Services
{
    public interface ISceneService
    {
        void LoadScene(string sceneName);
        UniTask LoadSceneAsync(string sceneName);
        void ReloadCurrentScene();
    }
}
