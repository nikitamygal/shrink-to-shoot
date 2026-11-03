using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Tools
{
    public class InjectMonoBehaviour : MonoBehaviour
    {
        private bool _injected;
        private bool _initialized;
        private bool _started;
        private bool _disposed;

        public bool IsInjected => _injected;
        public bool IsInitialized => _initialized;
        public bool IsStarted => _started;
        public bool IsDisposed => _disposed;

        public void Inject(IObjectResolver resolver)
        {
            if (_injected || resolver == null)
                return;

            resolver.Inject(this);
            MarkInjected();
        }

        public void MarkInjected()
        {
            if (_injected)
                return;

            _injected = true;
            TrackDisposeOnDestroy();
        }

        public void RunInitialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (this is IInitializable initializable)
                initializable.Initialize();
        }

        public void RunStart()
        {
            if (_started)
                return;

            _started = true;

            if (this is IStartable startable)
                startable.Start();
        }

        public void RunDispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (this is IDisposable disposable)
                disposable.Dispose();
        }

        private void TrackDisposeOnDestroy()
        {
            if (!(this is IDisposable))
                return;

            var tracker = GetComponent<InjectMonoBehaviourDisposeTracker>();
            if (tracker == null)
                tracker = gameObject.AddComponent<InjectMonoBehaviourDisposeTracker>();

            tracker.Track(this);
        }
    }

    [DisallowMultipleComponent]
    public sealed class InjectMonoBehaviourDisposeTracker : MonoBehaviour
    {
        private readonly List<InjectMonoBehaviour> _tracked = new();

        public void Track(InjectMonoBehaviour behaviour)
        {
            if (behaviour == null || _tracked.Contains(behaviour))
                return;

            _tracked.Add(behaviour);
        }

        private void OnDestroy()
        {
            for (int i = _tracked.Count - 1; i >= 0; i--)
            {
                InjectMonoBehaviour behaviour = _tracked[i];
                if (behaviour == null)
                    continue;

                try
                {
                    behaviour.RunDispose();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, behaviour);
                }
            }
        }
    }
}
