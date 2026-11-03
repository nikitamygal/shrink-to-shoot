using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Configs;
using Game.Events;
using Game.Levels;
using Game.UI;
using MoreMountains.Tools;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public class GameplayService : IGameplayService, IInitializable, IStartable, IDisposable
    {
        public MMStateMachine<GameStates> GameState { get; set; }
        public GameplayConfig Config { get; set; }

        private CancellationTokenSource _gameOverCancellation;

        [Inject] private readonly ILevelService _levelService;
        [Inject] private readonly IEventBus _eventBus;
        [Inject] private readonly IGUIService _guiService;
        
        [Inject]
        public GameplayService(GameplayConfig gameplayConfig)
        {
            GameState = new MMStateMachine<GameStates>(null, true);
            Config = gameplayConfig;
        }

        #region START STATE
        protected void OnStartState()
        {
            _levelService.BuildLevel();
            GameState.ChangeState(GameStates.Play);
        }
        #endregion

        #region PLAY STATE
        protected void OnPlayState()
        {

        }
        #endregion

        #region WIN STATE
        protected void OnWinState()
        {
            _gameOverCancellation?.Cancel();
            _gameOverCancellation = new CancellationTokenSource();
            WinStateSequence(_gameOverCancellation).Forget();
        }

        private async UniTask WinStateSequence(CancellationTokenSource cancellation)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Config.WinDelay), cancellationToken: cancellation.Token);
                // if (GameState.CurrentState == GameStates.Win)
                //     _guiService.Open<UIWinPanel>();
            }
            finally
            {
                if (ReferenceEquals(_gameOverCancellation, cancellation)) _gameOverCancellation = null;
                cancellation.Dispose();
            }
        }
        #endregion


        private void OnStateChange()
        {
            switch (GameState.CurrentState)
            {
                case GameStates.Start:
                    OnStartState();
                    break;
                case GameStates.Play:
                    OnPlayState();
                    break;
                case GameStates.Win:
                    OnWinState();
                    break;
            }
        }

        void IInitializable.Initialize()
        {
            GameState.OnStateChange += OnStateChange;
        }

        void IStartable.Start()
        {
            GameState.ChangeState(GameStates.Start);
        }

        void IDisposable.Dispose()
        {
            GameState.OnStateChange -= OnStateChange;
        }

    }
}
