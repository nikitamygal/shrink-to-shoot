using UnityEngine;
using VContainer;

namespace Game.UI.PopupWindow
{
    public class PopupWindowService : IPopupWindowService
    {
        private readonly PopupWindow _popupWindow;
        private readonly IObjectResolver _resolver;

        private bool _isOpened;
        private UIPanel _currentContent;

        public bool IsOpened => _isOpened;

        [Inject]
        public PopupWindowService(
            PopupWindow popupWindow,
            IObjectResolver resolver)
        {
            _popupWindow = popupWindow;
            _resolver = resolver;

            _popupWindow.BindClose(HandleCloseRequested);
        }

        public T Open<T>(T prefab, string title = null) where T : UIPanel
        {
            if (_isOpened)
                Close();

            _popupWindow.ClearContent();

            var instance = Object.Instantiate(prefab, _popupWindow.ContentRoot);

            _resolver.Inject(instance);
            instance.Init(_resolver);

            _currentContent = instance;

            string finalTitle = (_currentContent as IPopupContent)?.PopupTitle ?? title;
            _popupWindow.SetTitle(finalTitle);

            _popupWindow.gameObject.SetActive(true);

            instance.Show();
            _popupWindow.RefreshLayoutAndResize(instance.transform as RectTransform);
            _popupWindow.PlayOpen();

            _isOpened = true;
            return instance;
        }

        public void Close()
        {
            if (!_isOpened)
                return;

            _currentContent?.Hide();
            _currentContent = null;

            _popupWindow.PlayClose(() =>
            {
                _popupWindow.ClearContent();
                _popupWindow.gameObject.SetActive(false);
                _isOpened = false;
            });
        }

        private void HandleCloseRequested()
        {
            if (_currentContent is IPopupCloseHandler popupCloseHandler &&
                popupCloseHandler.HandlePopupCloseRequested())
            {
                return;
            }

            Close();
        }
    }
}
