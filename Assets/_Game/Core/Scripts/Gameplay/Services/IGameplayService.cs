using Game.Configs;
using MoreMountains.Tools;

namespace Game.Gameplay
{
    public interface IGameplayService
    {
        public MMStateMachine<GameStates> GameState { get; set; }
        public GameplayConfig Config { get; set; }
    }
}
