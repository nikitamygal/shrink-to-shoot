using Game.CameraTools;
using Game.Core.Interfaces;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace Game.Events.Gameplay
{
    public record ChangeCameraPriority(CameraSettings settings) : IGameEvent;
    public record SetCameraTarget(CameraTarget target) : IGameEvent;
}
