using Game.Core.Interfaces;
using Game.Core.SaveLoad;
using Game.Events;
using Game.GameFSM;
using Game.GameFSM.States;
using Game.Services;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Core
        builder.Register<GameStateMachine>(Lifetime.Singleton).AsSelf().As<IStateTransition>().As<IStateTransitionInternal>();

        // Services
        builder.Register<StateSceneMap>(Lifetime.Singleton).As<IStateSceneMap>();
        builder.Register<MMSceneService>(Lifetime.Singleton).As<ISceneService>();
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<SaveServiceBinary>(Lifetime.Singleton).As<ISaveLoadService>();

        // Game States
        builder.Register<EntryState>(Lifetime.Singleton);
        builder.Register<LoadingState>(Lifetime.Singleton);
        builder.Register<GameplayState>(Lifetime.Singleton);
    }
}
