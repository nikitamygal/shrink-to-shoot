using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterAllComponentsInChildrenAs<T>(this IContainerBuilder builder, LifetimeScope scope, bool includeInactive = true) where T : Component
        {
            var components = scope.GetComponentsInChildren<T>(includeInactive);
            foreach (var component in components)
            {
                builder.RegisterComponent(component).As<T>();
            }
        }

    }
}
