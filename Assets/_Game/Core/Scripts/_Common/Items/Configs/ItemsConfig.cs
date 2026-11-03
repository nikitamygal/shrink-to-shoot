using System.Collections.Generic;
using Game.Configs;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Grid
{
    [CreateAssetMenu(menuName = "Game/Configs/ItemsConfig", fileName = "ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private List<ItemSettingsConfig> _items = new();

        private Dictionary<ItemType, ItemSettingsConfig> _cache;

        public bool TryGetConfig(ItemType uniqueKey, out ItemSettingsConfig config)
        {
            BuildCache();
            return _cache.TryGetValue(uniqueKey, out config);
        }

        private void BuildCache()
        {
            if (_cache != null)
                return;

            _cache = new Dictionary<ItemType, ItemSettingsConfig>();

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(item.UniqueKey))
                    continue;

                _cache[item.Type] = item;
            }
        }
    }
}