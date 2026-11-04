using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Configs;
using Game.Events;
using Game.Events.Gameplay;
using Game.Levels;
using Game.UI;
using MoreMountains.Tools;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public class GameplayService : IGameplayService, IInitializable, IStartable, IDisposable
    {
        public MMStateMachine<GameStates> GameState { get; set; }
        public GameplayConfig Config { get; set; }

        private CancellationTokenSource _gameOverCancellation;
        private bool _disposed;

        [Inject] private readonly ILevelService _levelService;
        [Inject] private readonly IEventBus _eventBus;
        [Inject] private readonly IGUIService _guiService;

        [Inject]
        public GameplayService(GameplayConfig gameplayConfig)
        {
            GameState = new MMStateMachine<GameStates>(null, true);
            Config = gameplayConfig;
        }

        private void OnStateChange()
        {
            _gameOverCancellation?.Cancel();
            switch (GameState.CurrentState)
            {
                case GameStates.Start:
                    _levelService.BuildLevel();
                    GameState.ChangeState(GameStates.Play);
                    break;
                case GameStates.Win:
                case GameStates.Lose:
                    var cancellation = new CancellationTokenSource();
                    _gameOverCancellation = cancellation;
                    CompleteGameAsync(GameState.CurrentState, cancellation).Forget(Debug.LogException);
                    break;
            }
        }

        private async UniTask CompleteGameAsync(GameStates result, CancellationTokenSource cancellation)
        {
            try
            {
                float delay = result == GameStates.Win ? Config.WinDelay : Config.LoseDelay;
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, delay)), cancellationToken: cancellation.Token);
                if (!_disposed && GameState.CurrentState == result)
                {
                    if (result == GameStates.Win)
                        _guiService.Open<UIWinPanel>();
                    else if (result == GameStates.Lose)
                        _guiService.Open<UILosePanel>();

                    _eventBus.Trigger(new GameplayEnded(result));
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            finally
            {
                if (ReferenceEquals(_gameOverCancellation, cancellation)) _gameOverCancellation = null;
                cancellation.Dispose();
            }
        }

        void IInitializable.Initialize()
        {
            GameState.OnStateChange += OnStateChange;
        }

        void IStartable.Start() => GameState.ChangeState(GameStates.Start);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            GameState.OnStateChange -= OnStateChange;
            // The async owner's finally block disposes the source after cancellation is observed.
            _gameOverCancellation?.Cancel();
        }
    }
}
