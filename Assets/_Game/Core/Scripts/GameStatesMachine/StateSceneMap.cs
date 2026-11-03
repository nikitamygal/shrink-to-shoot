using System;
using System.Collections.Generic;
using Game.Core.Constants;
using Game.Core.Interfaces;
using Game.GameFSM.States;

namespace Game.GameFSM
{
    public class StateSceneMap : IStateSceneMap
    {
        private readonly Dictionary<Type, string> _map = new()
        {
            [typeof(GameplayState)] = Scenes.GamePlay
            // EntryState has no scene
            // LoadingState is a technical state (we don't route to it)
        };

        public bool TryGetScene(Type stateType, out string sceneName)
        {
            return _map.TryGetValue(stateType, out sceneName);
        }

        public bool TryGetStateForScene(string sceneName, out Type stateType)
        {
            foreach (var pair in _map)
            {
                if (pair.Value == sceneName)
                {
                    stateType = pair.Key;
                    return true;
                }
            }

            stateType = null;
            return false;
        }
    }
}
