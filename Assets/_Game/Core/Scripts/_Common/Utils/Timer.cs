using System;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace Game.Utils
{
    public class Timer
    {
        private float _duration;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public event Action OnCompleted;

        public Timer(float duration)
        {
            _duration = duration;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _cts = new CancellationTokenSource();
            RunTimerAsync(_cts.Token).Forget();
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cts?.Cancel();
            _cts = null;
        }

        public void Restart(float? newDuration = null)
        {
            Stop();
            if (newDuration.HasValue)
                _duration = newDuration.Value;
            Start();
        }

        private async UniTaskVoid RunTimerAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_duration), cancellationToken: token);
                if (!_isRunning || token.IsCancellationRequested)
                    return;

                _isRunning = false;
                OnCompleted?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
