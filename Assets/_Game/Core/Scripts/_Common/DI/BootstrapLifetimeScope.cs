using Game.Bootstrap;
using VContainer;
using VContainer.Unity;

public class BootstrapLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<GameBootstrapper>(Lifetime.Singleton);
    }
}
