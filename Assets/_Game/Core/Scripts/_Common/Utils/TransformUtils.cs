using UnityEngine;

namespace Game.Utils
{
    public static class TransformUtils
    {
        public static void ClearChilren(Component component)
        {
            if (component == null)
                return;

            for (int i = component.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = component.transform.GetChild(i);
                if (Application.isPlaying)
                    Object.Destroy(child.gameObject);
                else
                    Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
