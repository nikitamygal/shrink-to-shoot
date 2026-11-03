using UnityEngine;

namespace Game.UI.PopupWindow
{
    public interface IPopupWindowService
    {
        T Open<T>(T prefab, string title = null) where T : UIPanel;
        void Close();
        bool IsOpened { get; }
    }
}
