using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class UIStars : MonoBehaviour
    {
        [SerializeField] private GameObject _starFilledPrefab;
        [SerializeField] private GameObject _starEmptyPrefab;
        [SerializeField] private Transform _starsParent;

        private readonly List<GameObject> _stars = new();

        /// <summary>
        /// Draw stars based on current and max level
        /// </summary>
        public void SetStars(int currentLevel, int maxLevel)
        {
            ClearStars();

            currentLevel = Mathf.Clamp(currentLevel, 0, maxLevel);

            for (int i = 0; i < maxLevel; i++)
            {
                GameObject prefab = i < currentLevel
                    ? _starFilledPrefab
                    : _starEmptyPrefab;

                GameObject star = Instantiate(prefab, _starsParent);
                _stars.Add(star);
            }
        }

        private void ClearStars()
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                if (_stars[i] != null)
                    Destroy(_stars[i]);
            }

            _stars.Clear();
        }
    }
}
