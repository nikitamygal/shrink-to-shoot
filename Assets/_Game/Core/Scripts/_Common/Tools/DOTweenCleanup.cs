using UnityEngine;
using DG.Tweening;

namespace Game.Tools
{
    public class DOTweenCleanup : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            DOTween.KillAll(true);
            DOTween.Clear(true);
        }

    #if UNITY_EDITOR
        private void OnDisable()
        {
            // Called when exiting PlayMode in Editor
            DOTween.KillAll(true);
            DOTween.Clear(true);
        }
    #endif
    }
}
