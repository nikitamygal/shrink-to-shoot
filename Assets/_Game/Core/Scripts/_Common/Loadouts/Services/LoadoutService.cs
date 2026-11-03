using Game.Configs;

namespace Game.Core.Loadouts
{
    public class LoadoutService : ILoadoutService
    {
        private readonly GameplayConfig _gameplayConfig;

        public LoadoutService(GameplayConfig gameplayConfig)
        {
            _gameplayConfig = gameplayConfig;
        }

        public ItemSettingsConfig GetSelectedItem(LoadoutSlotType slotType)
        {
            if (_gameplayConfig?.DefaultLoadout != null
                && _gameplayConfig.DefaultLoadout.TryGetValue(slotType, out ItemSettingsConfig defaultItem))
            {
                return defaultItem;
            }

            return null;
        }
    }
}
