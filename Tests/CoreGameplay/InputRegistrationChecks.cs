using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

// Uses the installed VContainer assembly with simulated Unity input/focus APIs.
// Lifecycle collections are invoked explicitly: this does not test Unity's PlayerLoop.
public static class InputRegistrationChecks
{
    private static int _checks;

    private static void Assert(bool condition, string message)
    {
        _checks++;
        if (!condition) throw new Exception(message);
    }

    private static int SubscriberCount(Type owner, string eventName)
    {
        var handlers = owner.GetField(eventName, BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as Delegate;
        return handlers?.GetInvocationList().Length ?? 0;
    }

    public static void Main()
    {
        Application.SetFocus(true);
        Mouse.current = new Mouse();
        Touchscreen.current = new Touchscreen();
        var builder = new ContainerBuilder();
        builder.Register<InputService>(Lifetime.Singleton)
            .As<IInputService>().As<IInitializable>().As<ITickable>().As<IDisposable>();

        var container = builder.Build();
        try
        {
            var input = container.Resolve<IInputService>();
            var initializers = container.Resolve<IReadOnlyList<IInitializable>>();
            var tickers = container.Resolve<IReadOnlyList<ITickable>>();
            var disposables = container.Resolve<IReadOnlyList<IDisposable>>();
            Assert(initializers.Count == 1, "Exactly one input initializer is registered");
            Assert(tickers.Count == 1, "Exactly one input ticker is registered");
            Assert(disposables.Count == 1, "Exactly one input disposable is registered");
            Assert(ReferenceEquals(input, initializers[0]), "Initialization uses the gameplay input singleton");
            Assert(ReferenceEquals(input, tickers[0]), "Tick uses the gameplay input singleton");
            Assert(ReferenceEquals(input, disposables[0]), "Disposal uses the gameplay input singleton");
            Assert(ReferenceEquals(input, container.Resolve<IInputService>()), "Repeated resolution returns the same input");

            int starts = 0, releases = 0, cancellations = 0;
            input.ChargeStarted += () => starts++;
            input.ChargeReleased += () => releases++;
            input.ChargeCancelled += () => cancellations++;
            initializers[0].Initialize();
            initializers[0].Initialize();
            Assert(SubscriberCount(typeof(Application), "focusChanged") == 1,
                "Repeated initialization subscribes to focus exactly once");
            Assert(SubscriberCount(typeof(InputSystem), "onDeviceChange") == 1,
                "Repeated initialization subscribes to devices exactly once");
            tickers[0].Tick();
            Mouse.current.leftButton.isPressed = true;
            tickers[0].Tick();
            Assert(starts == 1, "The registered ticker emits a mouse charge event");
            for (int i = 0; i < 30; i++) tickers[0].Tick();
            Assert(starts == 1 && releases == 0, "The registered ticker does not repeat a held press");
            Mouse.current.leftButton.isPressed = false;
            tickers[0].Tick();
            Assert(releases == 1, "The registered ticker emits one release");
            Mouse.current.leftButton.isPressed = true;
            tickers[0].Tick();
            Assert(starts == 2, "A fresh press reaches the same registered service");

            container.Dispose();
            Assert(cancellations == 1, "Container disposal cancels the active charge");
            Assert(SubscriberCount(typeof(Application), "focusChanged") == 0 &&
                SubscriberCount(typeof(InputSystem), "onDeviceChange") == 0,
                "Container disposal removes focus and device subscriptions");
            Mouse.current.leftButton.isPressed = false;
            tickers[0].Tick();
            Mouse.current.leftButton.isPressed = true;
            tickers[0].Tick();
            Assert(starts == 2 && releases == 1, "Disposed registered input ignores further ticks");
            Application.SetFocus(false);
            InputSystem.ChangeDevice(Mouse.current, InputDeviceChange.Disconnected);
            Assert(cancellations == 1, "Post-disposal focus and device changes do not emit cancellation");
            Console.WriteLine(_checks + " VContainer input registration checks passed");
        }
        finally
        {
            container.Dispose();
        }
    }
}
