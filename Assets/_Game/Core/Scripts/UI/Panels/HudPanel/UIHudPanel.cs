using System;
using AYellowpaper.SerializedCollections;
using Game.Core.Interfaces;
using Game.GameFSM.States;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.UI
{
    public class UIHudPanel : UIPanel, IInitializable, IDisposable
    {
        public enum ElementType
        {
            BtnSettings = 1,
            BottomButtonGroup = 2,
            BtnReload = 3
        }

        [SerializeField] private SerializedDictionary<ElementType, GameObject> _elements = new();

        [Inject] private readonly IStateTransition _stateTransition;

        private BtnTouch _reloadButton;
        private bool _isReloading;

        void IInitializable.Initialize()
        {
            if (_reloadButton != null) _reloadButton.OnClicked -= ReloadLevel;
            _reloadButton = _elements.TryGetValue(ElementType.BtnReload, out var reload) && reload != null
                ? reload.GetComponent<BtnTouch>() : null;
            if (_reloadButton != null) _reloadButton.OnClicked += ReloadLevel;
        }

        private void ReloadLevel()
        {
            if (_isReloading) return;
            _isReloading = true;
            _stateTransition.Enter<GameplayState>();
        }

        void IDisposable.Dispose()
        {
            if (_reloadButton != null) _reloadButton.OnClicked -= ReloadLevel;
            _reloadButton = null;
        }

        public void SetElementVisible(ElementType elementType, bool visible)
        {
            if (_elements.TryGetValue(elementType, out var element) && element != null)
                element.SetActive(visible);
        }

        public void SetElementsVisible(bool visible, params ElementType[] elementTypes)
        {
            foreach (var elementType in elementTypes)
                SetElementVisible(elementType, visible);
        }
    }
}
