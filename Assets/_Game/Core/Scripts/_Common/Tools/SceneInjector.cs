using Game.Tools;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Utils
{
    public class SceneInjector : IInitializable, IStartable
    {
        [Inject] private readonly IObjectResolver _resolver;
        private InjectMonoBehaviour[] _injectMonos;

        void IInitializable.Initialize()
        {
            _injectMonos = Object.FindObjectsByType<InjectMonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (InjectMonoBehaviour mono in _injectMonos)
            {
                if (mono == null)
                    continue;

                mono.Inject(_resolver);
            }

            foreach (InjectMonoBehaviour mono in _injectMonos)
            {
                if (mono == null)
                    continue;

                mono.RunInitialize();
            }
        }

        void IStartable.Start()
        {
            if (_injectMonos == null)
                return;

            foreach (InjectMonoBehaviour mono in _injectMonos)
            {
                if (mono == null)
                    continue;

                mono.RunStart();
            }
        }
    }
}
