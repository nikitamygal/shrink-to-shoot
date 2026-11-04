using Game.CameraTools;
using Game.Configs;
using Game.Core.Loadouts;
using Game.Levels;
using Game.Tools;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Spawners
{
    public class PlayerSpawnPoint : InjectMonoBehaviour, IStartable
    {
        private ILevelService _levelService;
        private ILoadoutService _loadoutService;
        private IObjectResolver _resolver;
        private bool _spawned;

        [Inject]
        public void Construct(ILevelService levelService, ILoadoutService loadoutService, IObjectResolver resolver)
        {
            _levelService = levelService;
            _loadoutService = loadoutService;
            _resolver = resolver;
        }

        void IStartable.Start()
        {
            if (_spawned || _levelService == null || _resolver == null) return;
            _spawned = true;
            CameraTarget target = SpawnPlayer();
            _levelService?.RegisterMainTarget(target);
        }

        protected CameraTarget SpawnPlayer()
        {
            ItemSettingsConfig targetConfig = _loadoutService?.GetSelectedItem(LoadoutSlotType.Player);
            if (targetConfig == null || targetConfig.Prefab == null)
                return null;

            GameObject player = Instantiate(targetConfig.Prefab, transform.position, transform.rotation);
            CameraTarget target = player.GetComponent<CameraTarget>() ?? player.GetComponentInChildren<CameraTarget>();
            if (target == null)
            {
                Destroy(player);
                return null;
            }

            _resolver?.InjectAndInitialize(player);
            return target;
        }
    }
}
