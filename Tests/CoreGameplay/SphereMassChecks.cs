using System;
using Game.Core.Controllers;

// Standalone executable checks: no Unity runtime or test-package installation required.
public static class SphereMassChecks
{
    private static int _checks;
    private static void Assert(bool condition, string message)
    {
        _checks++;
        if (!condition) throw new Exception(message);
    }
    private static void Near(float actual, float expected, string message)
        => Assert(Math.Abs(actual - expected) <= Math.Max(0.00001f, Math.Abs(expected) * 0.00002f), message);
    private static void Reject(Action action)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException) { _checks++; return; }
        throw new Exception("Invalid input was accepted");
    }
    public static void Main()
    {
        foreach (float radius in new[] { 0f, .01f, .12f, .2f, .75f, 1f, 10f })
            Near(SphereMass.ToRadius(SphereMass.FromRadius(radius)), radius, "Radius round trip");
        Near(SphereMass.FromRadius(2f), 8f * SphereMass.FromRadius(1f), "Volume scales cubically");
        Reject(() => SphereMass.FromRadius(-1));
        Reject(() => SphereMass.FromRadius(float.NaN));
        Reject(() => SphereMass.FromRadius(float.PositiveInfinity));
        Reject(() => SphereMass.FromRadius(float.MaxValue));
        Reject(() => SphereMass.ToRadius(-1));
        Reject(() => SphereMass.ToRadius(float.NaN));
        Reject(() => new SphereMassTransfer(.2f, .2f));
        Reject(() => new SphereMassTransfer(.2f, 0));
        Reject(() => new SphereMassTransfer(.2f, float.NaN));

        var mass = new SphereMassTransfer(.75f, .2f);
        float initial = mass.InitialMass;
        Near(mass.Transfer(1), 0, "Transfer requires Begin");
        Assert(mass.Begin(), "First Begin succeeds");
        Assert(!mass.Begin(), "Duplicate Begin rejected");
        mass.Transfer(.1f);
        Near(mass.CurrentMass + mass.PendingMass, initial, "Preview conserves mass");
        mass.Cancel();
        Near(mass.CurrentMass, initial, "Cancellation refunds exactly");
        Near(mass.PendingMass, 0, "Cancellation clears pending");
        mass.Cancel();
        Near(mass.CurrentMass, initial, "Repeated cancellation is harmless");
        Assert(mass.Begin(), "Begin after cancellation");
        Reject(() => mass.Transfer(-1));
        Reject(() => mass.Transfer(float.NaN));
        Reject(() => mass.Transfer(float.PositiveInfinity));
        mass.Transfer(.2f);
        float shot = mass.Commit();
        Near(mass.CurrentMass + shot, initial, "Commit conserves mass");
        Near(mass.Commit(), 0, "One shot per transaction");
        mass.Cancel();
        Near(mass.CurrentMass + shot, initial, "Committed shot cannot be refunded");

        foreach (int fps in new[] { 30, 60, 120 })
        {
            var timed = new SphereMassTransfer(.75f, .2f);
            timed.Begin();
            for (int i = 0; i < fps; i++) timed.Transfer(.35f / fps);
            Near(timed.PendingMass, .35f, "Frame-rate independent charging");
            Near(timed.CurrentMass + timed.PendingMass, timed.InitialMass, "Timed conservation");
        }
        var critical = new SphereMassTransfer(.75f, .2f);
        critical.Begin();
        critical.Transfer(float.MaxValue);
        Near(critical.CurrentMass, critical.MinimumMass, "Critical clamp");
        Assert(critical.IsCritical, "Critical threshold detected");
        Near(critical.CurrentMass + critical.PendingMass, critical.InitialMass, "Critical conservation");
        critical.Commit();
        Assert(!critical.Begin(), "Cannot charge below critical threshold");
        Console.WriteLine(_checks + " sphere-mass checks passed");
    }
}
