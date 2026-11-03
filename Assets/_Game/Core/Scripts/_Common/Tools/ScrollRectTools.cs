using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tools
{
    public static class ScrollRectTools
    {
        public static async UniTask ScrollToTarget(ScrollRect scrollRect, RectTransform target, float duration = 0.35f)
        {
            if (scrollRect == null || target == null)
                return;

            var content = scrollRect.content;
            var viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : scrollRect.GetComponent<RectTransform>();

            Canvas.ForceUpdateCanvases();

            Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, content);
            Bounds targetBounds  = RectTransformUtility.CalculateRelativeRectTransformBounds(content, target);

            float contentHeight  = contentBounds.size.y;
            float viewportHeight = viewport.rect.height;

            float scrollable = contentHeight - viewportHeight;
            if (scrollable <= 0.01f)
                return;

            float contentTop = contentBounds.max.y;
            float targetTop  = targetBounds.max.y;

            float offsetFromTop = contentTop - targetTop;
            float targetNormalized = 1f - Mathf.Clamp01(offsetFromTop / scrollable);

            float start = scrollRect.verticalNormalizedPosition;
            float time  = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, time / duration);
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, targetNormalized, t);
                await UniTask.Yield();
            }

            scrollRect.verticalNormalizedPosition = targetNormalized;
        }

        public static async UniTask WaitForLayoutAsync(ScrollRect _scrollRect)
        {
            if (_scrollRect == null || _scrollRect.content == null)
                return;

            // Wait one frame for VerticalLayoutGroup to rebuild
            await UniTask.NextFrame();

            // Force layout rebuild (important for ContentSizeFitter)
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Canvas.ForceUpdateCanvases();

            // Sometimes ContentSizeFitter needs one more frame
            await UniTask.NextFrame();

            Canvas.ForceUpdateCanvases();
        }
    }
}
