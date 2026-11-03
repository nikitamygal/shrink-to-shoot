using UnityEngine;

namespace Game.UI
{
    [DisallowMultipleComponent]
    public class UISafeAreaOffset : MonoBehaviour
    {
        [System.Flags]
        private enum Insets
        {
            None = 0,
            Top = 1,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
        }

        [SerializeField] private RectTransform _referenceRect;
        [SerializeField] private RectTransform[] _targets;
        [SerializeField] private Insets _insets = Insets.Top;
        [SerializeField] private float _extraTopPadding;
        [SerializeField] private float _extraBottomPadding;
        [SerializeField] private float _extraLeftPadding;
        [SerializeField] private float _extraRightPadding;

        private Vector2[] _defaultAnchoredPositions;
        private RectTransform[] _cachedTargets;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private Vector2 _lastReferenceSize = new Vector2(-1f, -1f);

        private void Awake()
        {
            CacheDefaultPositions(force: true);
            ApplySafeArea(force: true);
        }

        private void OnEnable()
        {
            ApplySafeArea(force: true);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;

            ApplySafeArea();
        }

        public void Refresh()
        {
            ApplySafeArea(force: true);
        }

        private void CacheDefaultPositions(bool force = false)
        {
            var targets = ResolveTargets();
            if (!force &&
                _cachedTargets != null &&
                _defaultAnchoredPositions != null &&
                _cachedTargets.Length == targets.Length)
            {
                var sameTargets = true;
                for (var i = 0; i < targets.Length; i++)
                {
                    if (_cachedTargets[i] == targets[i])
                        continue;

                    sameTargets = false;
                    break;
                }

                if (sameTargets)
                    return;
            }

            _cachedTargets = targets;
            _defaultAnchoredPositions = new Vector2[targets.Length];

            for (var i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                _defaultAnchoredPositions[i] = target != null
                    ? target.anchoredPosition
                    : Vector2.zero;
            }
        }

        private void ApplySafeArea(bool force = false)
        {
            CacheDefaultPositions();

            if (_cachedTargets == null || _cachedTargets.Length == 0)
                return;

            var referenceRect = ResolveReferenceRect();
            if (referenceRect == null)
                return;

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
            var referenceSize = referenceRect.rect.size;

            if (!force &&
                safeArea == _lastSafeArea &&
                screenSize == _lastScreenSize &&
                referenceSize == _lastReferenceSize)
            {
                return;
            }

            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
            _lastReferenceSize = referenceSize;

            var widthScale = referenceSize.x / screenSize.x;
            var heightScale = referenceSize.y / screenSize.y;

            var leftInset = safeArea.xMin * widthScale + _extraLeftPadding;
            var rightInset = (screenSize.x - safeArea.xMax) * widthScale + _extraRightPadding;
            var bottomInset = safeArea.yMin * heightScale + _extraBottomPadding;
            var topInset = (screenSize.y - safeArea.yMax) * heightScale + _extraTopPadding;

            for (var i = 0; i < _cachedTargets.Length; i++)
            {
                var target = _cachedTargets[i];
                if (target == null)
                    continue;

                var anchoredPosition = _defaultAnchoredPositions[i];

                if ((_insets & Insets.Top) != 0)
                    anchoredPosition.y -= topInset;

                if ((_insets & Insets.Bottom) != 0)
                    anchoredPosition.y += bottomInset;

                if ((_insets & Insets.Left) != 0)
                    anchoredPosition.x += leftInset;

                if ((_insets & Insets.Right) != 0)
                    anchoredPosition.x -= rightInset;

                target.anchoredPosition = anchoredPosition;
            }
        }

        private RectTransform ResolveReferenceRect()
        {
            if (_referenceRect != null)
                return _referenceRect;

            return transform as RectTransform;
        }

        private RectTransform[] ResolveTargets()
        {
            if (_targets != null && _targets.Length > 0)
                return _targets;

            var selfRect = transform as RectTransform;
            return selfRect != null
                ? new[] { selfRect }
                : System.Array.Empty<RectTransform>();
        }
    }
}
