using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.UI
{
    public class UIPanel : MonoBehaviour, IPanel
    {
        public event Action OnPanelOpen;
        public event Action OnPanelClose;
        
        private HashSet<UIElement> _injectedElements = new();
        private IObjectResolver _resolver;

        #region INIT/ON DESTROY
        public void Init(IObjectResolver resolver)
        {
            _resolver = resolver;
            _injectedElements ??= new HashSet<UIElement>();

            // Panel
            InitializePanel();

            // Elements
            InjectElements();
            InitializeElements();

            // Start after child elements are injected and initialized.
            StartPanel();
        }

        protected virtual void OnDestroy()
        {
            DisposeElements();
            DisposePanel();
        }
        #endregion

        #region SHOW/HIDE
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnPanelOpen?.Invoke();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnPanelClose?.Invoke();
        }
        #endregion

        private void InjectElements()
        {
            if (_resolver == null)
                return;

            _injectedElements ??= new HashSet<UIElement>();

            var elements = GetComponentsInChildren<UIElement>(includeInactive: true);
            foreach (var element in elements)
            {
                if (element == null || _injectedElements.Contains(element))
                    continue;

                _resolver.Inject(element);
                _injectedElements.Add(element);
            }
        }

        protected void InitializeElements()
        {
            foreach (var element in _injectedElements)
            {
                if (element == null)
                    continue;

                if (element is IInitializable initializable)
                    initializable.Initialize();
            }
        }

        protected void DisposeElements()
        {
            foreach (var element in _injectedElements)
            {
                if (element == null)
                    continue;

                if (element is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        protected void InitializePanel()
        {
            if (this is IInitializable initializable)
                initializable.Initialize();
        }

        protected void StartPanel()
        {
            if (this is IStartable startable)
                startable.Start();
        }
        
        protected void DisposePanel()
        {
            if (this is IDisposable disposable)
                disposable.Dispose();
        }

    }
}
