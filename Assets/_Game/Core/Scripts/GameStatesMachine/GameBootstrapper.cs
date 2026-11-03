using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.SaveLoad;
using Game.GameFSM;
using Game.GameFSM.States;
using VContainer.Unity;

namespace Game.Bootstrap
{
    public class GameBootstrapper : IAsyncStartable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ISaveLoadService _save;

        public GameBootstrapper(GameStateMachine stateMachine, ISaveLoadService save)
        {
            _stateMachine = stateMachine;
            _save = save;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            if (!_save.IsSaveDataLoaded)
                await _save.LoadDataAsync().AttachExternalCancellation(cancellation);
            cancellation.ThrowIfCancellationRequested();
            _stateMachine.Enter<EntryState>();
        }
    }
}

