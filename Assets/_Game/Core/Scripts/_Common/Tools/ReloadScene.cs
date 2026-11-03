using UnityEngine;
using UnityEngine.SceneManagement;


namespace Game.Tools
{
    public class ReloadScene : MonoBehaviour
    {
        private void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.R))
            {
                ReloadCurrentScene();
            }
#endif
        }

    }
}
