using UnityEngine;

namespace Game.Tools
{
    public class RandomObjectActivator : MonoBehaviour
    {
        [SerializeField] private GameObject[] _objects;

        public void ActivateRandom()
        {
            if (_objects == null || _objects.Length == 0)
            {
                Debug.LogWarning("RandomObjectActivator: empty array!");
                return;
            }

            // Выключаем все
            foreach (var obj in _objects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            int randomIndex = Random.Range(0, _objects.Length);
            if (_objects[randomIndex] != null)
                _objects[randomIndex].SetActive(true);
        }

        private void OnEnable()
        {
            ActivateRandom();
        }
    }

}
