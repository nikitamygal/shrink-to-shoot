using Game.UI;
using UnityEngine;

namespace Game.UI
{
    public interface IGUIService
    {
        public Canvas MainCanvas { get; }

        public T Get<T>() where T : UIPanel;
        public T Open<T>() where T : UIPanel;
        public void Close<T>() where T : UIPanel;
    }
}
