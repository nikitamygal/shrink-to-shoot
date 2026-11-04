using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.UI
{
    public class GUIService : IGUIService, IStartable
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<Type, UIPanel> _panels = new();
        public Canvas MainCanvas { get; private set; }

        public GUIService(IObjectResolver resolver, UIRoot uRoot)
        {
            _resolver = resolver;
            MainCanvas = uRoot.GetComponent<Canvas>();
        }

        public T Get<T>() where T : UIPanel
        {
            return _panels[typeof(T)] != null ? (T)_panels[typeof(T)] : null;
        }

        public T Open<T>() where T : UIPanel
        {
            if (!_panels.TryGetValue(typeof(T), out var panel))
            {
                panel = GameObject.FindFirstObjectByType<T>(FindObjectsInactive.Include);
                _resolver.Inject(panel);
                panel.Init(_resolver);

                _panels[typeof(T)] = panel;
            }

            panel.Show();
            return (T)panel;
        }

        public void Close<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
                panel.Hide();
        }

        private void AddPanels()
        {
            UIPanel[] panels = GameObject.FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach(UIPanel panel in panels)
            {
                Type panelType = panel.GetType();
                if (!_panels.TryGetValue(panel.GetType(), out var typedPanel))
                {
                    _resolver.Inject(panel);
                    panel.Init(_resolver);

                    _panels[panelType] = panel;
                }
            }
        }

        void IStartable.Start()
        {
            AddPanels();
        }
    }
}
