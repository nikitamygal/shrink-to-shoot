using UnityEngine;

namespace Game.UI
{
    public class UIElement : MonoBehaviour
    {
        public virtual void SetState(bool state)
        {
            this.gameObject.SetActive(state);
        }
    }
}
