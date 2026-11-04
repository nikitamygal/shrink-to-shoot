using System;
using Game.Core.Input;
using UnityEngine.InputSystem;

// Standalone input-edge checks. Device/focus APIs are simulated; Unity runtime is not claimed.
public static class InputServiceChecks
{
    private static int _checks;
    private static void Assert(bool condition, string message)
    {
        _checks++;
        if (!condition) throw new Exception(message);
    }
    private sealed class Session : IDisposable
    {
        public readonly InputService Input = new InputService();
        public int Starts, Releases, Cancels;
        public Session()
        {
            UnityEngine.Application.SetFocus(true);
            Mouse.current = new Mouse();
            Touchscreen.current = new Touchscreen();
            Input.ChargeStarted += () => Starts++;
            Input.ChargeReleased += () => Releases++;
            Input.ChargeCancelled += () => Cancels++;
            Step(false);
        }
        public void Step(bool mouse, bool touch = false, TouchPhase phase = TouchPhase.Ended)
        {
            Mouse.current.leftButton.isPressed = mouse;
            Touchscreen.current.primaryTouch.press.isPressed = touch;
            Touchscreen.current.primaryTouch.phase.Value = phase;
            Input.Tick();
        }
        public void Dispose() => Input.Dispose();
    }
    public static void Main()
    {
        using (var s = new Session())
        {
            s.Step(true);
            Assert(s.Starts == 1, "Mouse press charges immediately after startup");
            for (int i = 0; i < 30; i++) s.Step(true);
            Assert(s.Starts == 1 && s.Releases == 0, "Holding never repeats or fires");
            s.Step(false);
            s.Step(false);
            Assert(s.Releases == 1, "One release per hold");
            s.Step(true);
            Assert(s.Starts == 2, "Next press starts next charge");
            s.Input.CancelCharge();
            s.Step(true);
            Assert(s.Cancels == 1 && s.Starts == 2, "Cancelled hold stays cancelled");
            s.Step(false);
            Assert(s.Releases == 1, "Cancelled hold never fires on release");
            s.Step(true);
            Assert(s.Starts == 3, "Fresh press after cancellation works");
            s.Input.Dispose();
            Assert(s.Cancels == 2, "Dispose cancels active charge");
            s.Step(false);
            s.Step(true);
            Assert(s.Starts == 3 && s.Releases == 1, "Disposed service emits no input");
        }
        using (var s = new Session())
        {
            s.Input.Initialize();
            s.Input.Initialize();
            s.Step(false, true, TouchPhase.Began);
            s.Step(false, true, TouchPhase.Stationary);
            Assert(s.Starts == 1, "Touch charges once; repeated initialization is harmless");
            s.Step(false);
            Assert(s.Releases == 1, "Touch release fires once");
            s.Step(false, true, TouchPhase.Began);
            s.Step(false, false, TouchPhase.Canceled);
            Assert(s.Cancels == 1 && s.Releases == 1, "Touch cancellation never fires");
            s.Step(false);
            s.Step(false, true, TouchPhase.Began);
            UnityEngine.Application.SetFocus(false);
            Assert(s.Cancels == 2, "Focus loss cancels charge");
            UnityEngine.Application.SetFocus(true);
            s.Step(false, true, TouchPhase.Stationary);
            Assert(s.Starts == 3, "Focus restore cannot reuse a held touch");
            s.Step(false);
            s.Step(true);
            InputSystem.ChangeDevice(Mouse.current, InputDeviceChange.Disconnected);
            Assert(s.Cancels == 3, "Device removal cancels charge");
        }
        using (var s = new Session())
        {
            s.Step(true);
            s.Step(true, false, TouchPhase.Canceled);
            Assert(s.Cancels == 0, "Inactive cancelled touchscreen cannot cancel mouse");
            s.Step(true, true, TouchPhase.Began);
            Assert(s.Starts == 1, "Simultaneous sources never duplicate start");
            s.Step(false, true, TouchPhase.Stationary);
            Assert(s.Releases == 0, "Remaining held touch prevents premature release");
            s.Step(false);
            Assert(s.Releases == 1, "Last combined source release fires once");
        }
        using (var s = new Session())
        {
            s.Step(true);
            s.Step(true, true, TouchPhase.Began);
            s.Step(false, false, TouchPhase.Canceled);
            Assert(s.Cancels == 1 && s.Releases == 0, "Touch cancellation after handoff never fires");
            s.Step(false);
            s.Step(true, false, TouchPhase.Canceled);
            Assert(s.Starts == 2 && s.Cancels == 1, "Next mouse charge ignores stale touch cancellation");
        }
        using (var s = new Session())
        {
            // Check a focus snapshot change even if Unity's focus event was missed.
            UnityEngine.Application.isFocused = false;
            s.Step(true);
            Assert(s.Starts == 0, "Background polling cannot charge");
            UnityEngine.Application.isFocused = true;
            s.Step(false);
            s.Step(true);
            Assert(s.Starts == 1, "A current focus snapshot allows the next fresh press");
        }
        Console.WriteLine(_checks + " input transition checks passed");
    }
}

#if !REAL_VCONTAINER
namespace VContainer.Unity
{
    public interface IInitializable { void Initialize(); }
    public interface ITickable { void Tick(); }
}
#endif
namespace UnityEngine
{
    public static class Application
    {
        public static bool isFocused = true;
        public static event Action<bool> focusChanged;
        public static void SetFocus(bool value) { isFocused = value; focusChanged?.Invoke(value); }
    }
}
namespace UnityEngine.InputSystem
{
    public enum TouchPhase { None, Began, Moved, Stationary, Ended, Canceled }
    public enum InputDeviceChange { Added, Disconnected, Removed, Disabled }
    public class InputDevice { }
    public sealed class Button { public bool isPressed; }
    public sealed class Phase { public TouchPhase Value; public TouchPhase ReadValue() => Value; }
    public sealed class Touch { public readonly Button press = new Button(); public readonly Phase phase = new Phase(); }
    public sealed class Mouse : InputDevice { public static Mouse current; public readonly Button leftButton = new Button(); }
    public sealed class Touchscreen : InputDevice { public static Touchscreen current; public readonly Touch primaryTouch = new Touch(); }
    public static class InputSystem
    {
        public static event Action<InputDevice, InputDeviceChange> onDeviceChange;
        public static void ChangeDevice(InputDevice device, InputDeviceChange change) => onDeviceChange?.Invoke(device, change);
    }
}
