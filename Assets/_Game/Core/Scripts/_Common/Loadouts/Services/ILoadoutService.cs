using Game.Configs;

namespace Game.Core.Loadouts
{
    public interface ILoadoutService
    {
        ItemSettingsConfig GetSelectedItem(LoadoutSlotType slotType);
    }
}
