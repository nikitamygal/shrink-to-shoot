using UnityEngine;
using UnityEngine.UI;

namespace SoloGames {
    
    [RequireComponent(typeof(Image))]
    public class UISpriteAnimation : MonoBehaviour
    {
        public Sprite[] sprites;
        public int framesPerSprite = 6;
        public bool loop = true;
        public bool destroyOnEnd = false;

        private int index = 0;
        private Image image;
        private int frame = 0;

        void Awake() {
            image = GetComponent<Image>();
        }

        void Update () {
            if (!loop && index == sprites.Length)
            return;
            frame++;
            if (frame < framesPerSprite)
                return;
            image.sprite = sprites[index];
            frame = 0;
            index++;
            if (index >= sprites.Length)
            {
                if (loop)
                    index = 0;
                if (destroyOnEnd)
                    Destroy(gameObject);
            }
        }
    }

}


