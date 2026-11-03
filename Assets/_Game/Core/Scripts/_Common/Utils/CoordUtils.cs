using UnityEngine;

namespace Game.Utils
{
    /// <summary>
    /// Utility methods for converting positions between
    /// World, Screen and UI (RectTransform local) spaces.
    /// Safe for ScreenSpaceOverlay, ScreenSpaceCamera and WorldSpace canvases.
    /// </summary>
    public static class CoordUtils
    {
        #region === PUBLIC HIGH-LEVEL API ===

        /// <summary>
        /// Converts UI RectTransform position to local position inside target RectTransform.
        /// </summary>
        public static Vector2 UIToLocal(
            RectTransform uiElement,
            RectTransform targetRoot,
            Canvas canvas)
        {
            if (uiElement == null || targetRoot == null || canvas == null)
                return Vector2.zero;

            Vector2 screenPos = UIToScreen(uiElement, canvas);
            return ScreenToUILocal(screenPos, targetRoot, canvas);
        }

        /// <summary>
        /// Converts world position to local UI position inside target RectTransform.
        /// </summary>
        public static Vector2 WorldToLocal(
            Vector3 worldPosition,
            RectTransform targetRoot,
            Camera worldCamera,
            Canvas canvas)
        {
            if (targetRoot == null || canvas == null || worldCamera == null)
                return Vector2.zero;

            Vector2 screenPos = WorldToScreen(worldPosition, worldCamera);
            return ScreenToUILocal(screenPos, targetRoot, canvas);
        }

        #endregion

        #region === UI → SCREEN ===

        /// <summary>
        /// Converts UI RectTransform position to screen position.
        /// </summary>
        public static Vector2 UIToScreen(
            RectTransform uiElement,
            Canvas canvas)
        {
            Camera cam = GetCanvasCamera(canvas);
            return RectTransformUtility.WorldToScreenPoint(cam, uiElement.position);
        }

        #endregion

        #region === WORLD → SCREEN ===

        /// <summary>
        /// Converts world position to screen position.
        /// </summary>
        public static Vector2 WorldToScreen(
            Vector3 worldPosition,
            Camera worldCamera)
        {
            return worldCamera.WorldToScreenPoint(worldPosition);
        }

        #endregion

        #region === SCREEN → UI LOCAL ===

        /// <summary>
        /// Converts screen position to local position inside target RectTransform.
        /// </summary>
        public static Vector2 ScreenToUILocal(
            Vector2 screenPosition,
            RectTransform targetRoot,
            Canvas canvas)
        {
            Camera cam = GetCanvasCamera(canvas);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRoot,
                screenPosition,
                cam,
                out Vector2 localPoint
            );

            return localPoint;
        }

        #endregion

        #region === WORLD → UV ===

        public static Vector2 WorldToUV(Vector3 world, Canvas canvas, RectTransform root)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, world),
                canvas.worldCamera,
                out var local
            );

            Rect r = root.rect;

            return new Vector2(
                Mathf.InverseLerp(r.xMin, r.xMax, local.x),
                Mathf.InverseLerp(r.yMin, r.yMax, local.y)
            );
        }
        
        #endregion

        #region === SCREEN → UV ===

        public static Vector2 ScreenToUV(
            Vector2 screen,
            RectTransform root,
            Canvas canvas)
        {
            Camera cam = GetCanvasCamera(canvas);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root,
                screen,
                cam,
                out var local
            );

            Rect r = root.rect;

            return new Vector2(
                Mathf.InverseLerp(r.xMin, r.xMax, local.x),
                Mathf.InverseLerp(r.yMin, r.yMax, local.y)
            );
        }

        #endregion

        #region === INTERNAL HELPERS ===

        private static Camera GetCanvasCamera(Canvas canvas)
        {
            if (canvas == null)
                return null;

            return canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
        }

        #endregion
    }
}
