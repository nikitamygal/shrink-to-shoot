using Game.Tools;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public static class VContainerExtensions
{
    public static void InjectAndInitialize(this IObjectResolver resolver, GameObject gameObject)
    {
        resolver.InjectGameObject(gameObject);

        InjectMonoBehaviour[] injectMonos = gameObject.GetComponentsInChildren<InjectMonoBehaviour>(true);

        foreach (InjectMonoBehaviour mono in injectMonos)
        {
            if (mono == null)
                continue;

            mono.MarkInjected();
        }

        foreach (InjectMonoBehaviour mono in injectMonos)
        {
            if (mono == null)
                continue;

            mono.RunInitialize();
        }

        foreach (InjectMonoBehaviour mono in injectMonos)
        {
            if (mono == null)
                continue;

            mono.RunStart();
        }
    }
}
