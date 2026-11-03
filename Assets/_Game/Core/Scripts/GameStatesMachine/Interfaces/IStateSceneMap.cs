using System;

namespace Game.Core.Interfaces
{
    public interface IStateSceneMap
    {
        bool TryGetScene(Type stateType, out string sceneName);
        public bool TryGetStateForScene(string sceneName, out Type stateType);
    }
}
