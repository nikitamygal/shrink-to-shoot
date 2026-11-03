using UnityEngine;
using UnityEngine.UI;

namespace Game.Tools
{
    public class CustomCursor : MonoBehaviour
    {
        [SerializeField] private Image _cursorImage;
        [SerializeField] private Vector3 _offset;

        private void Start()
        {
            Cursor.visible = false;
        }

        private void Update()
        {
            MoveCursor();
        }

        private void MoveCursor()
        { 
            if (_cursorImage == null) return;
            _cursorImage.transform.position = Input.mousePosition + _offset;
        }
    }
}
