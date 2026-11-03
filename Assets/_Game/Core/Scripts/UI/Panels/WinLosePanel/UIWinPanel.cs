// using System.Collections.Generic;
// using Coffee.UIExtensions;
// using DG.Tweening;
// using UnityEngine;
// using UnityEngine.UI;
// using VContainer;
// using VContainer.Unity;

// namespace Game.UI
// {
//     [RequireComponent(typeof(CanvasGroup))]
//     public class UIWinPanel : UIGameOverPanel, IInitializable
//     {
//         private const float NoThanksDelay = 3f;
//         private static readonly string[] PraiseKeys =
//         {
//             "win.perfect", "win.awesome", "win.amazing", "win.excellent", "win.great"
//         };

//         [Header("Win visuals")]
//         [SerializeField] private Image _emojiImage;
//         [SerializeField] private List<Sprite> _emojiSprites = new();
//         [SerializeField, Min(0.01f)] private float _emojiDuration = 0.5f;
//         [SerializeField] private Ease _emojiEase = Ease.OutBounce;
//         [SerializeField] private Image _perfectImage;
//         [SerializeField] private LocalizeSpriteEvent _perfectLocalizer;
//         [SerializeField, Min(0.01f)] private float _perfectDuration = 0.5f;
//         [SerializeField] private Ease _perfectEase = Ease.OutElastic;
//         [SerializeField, Min(0.01f)] private float _appearDuration = 0.5f;
//         [SerializeField] private UIParticle _confetti;

//         [Header("Reward")]
//         [SerializeField] private TextMeshProUGUI _rewardText;
//         [SerializeField] private TextMeshProUGUI _tripleRewardText;
//         [SerializeField] private TextMeshProUGUI _multiplierText;
//         [SerializeField, Min(1)] private int _multiplier = 3;
//         [SerializeField] private BtnTouch _tripleRewardButton;
//         [SerializeField] private BtnTouch _noThanksButton;
//         [SerializeField] private RectTransform _rewardOrigin;
//         [SerializeField] private RectTransform _tripleRewardOrigin;

//         [Inject] private readonly IRewardFlyService _rewardFlyService;

//         private CanvasGroup _canvasGroup;
//         private Vector3 _emojiScale;
//         private Vector3 _perfectScale;
//         private Sequence _appearSequence;
//         private Tween _noThanksDelay;
//         private float _reward;
//         private bool _rewardClaimed;
//         private bool _transitionStarted;

//         void IInitializable.Initialize()
//         {
//             _canvasGroup = GetComponent<CanvasGroup>();
//             _emojiScale = _emojiImage != null ? _emojiImage.transform.localScale : Vector3.one;
//             _perfectScale = _perfectImage != null ? _perfectImage.transform.localScale : Vector3.one;
//             StopConfetti();
//         }

//         public override void Show()
//         {
//             if (_rewardClaimed)
//                 return;

//             StopAppearance();
//             _reward = Mathf.Max(0f, _levelService.LevelConfig.Reward.Value);
//             int multiplier = Mathf.Max(1, _multiplier);
//             if (_rewardText != null) _rewardText.text = _reward.ToString("0.##");
//             if (_tripleRewardText != null) _tripleRewardText.text = (_reward * multiplier).ToString("0.##");
//             Localize.Text(_multiplierText, "reward.multiplier", multiplier);

//             SetRandomSprite(_emojiImage, _emojiSprites);
//             if (_perfectLocalizer != null)
//                 _perfectLocalizer.AssetReference.SetReference(LocalizationTables.AssetTable,
//                     PraiseKeys[Random.Range(0, PraiseKeys.Length)]);
//             _tripleRewardButton?.Enable();
//             if (_noThanksButton != null)
//             {
//                 _noThanksButton.Disable();
//                 _noThanksButton.gameObject.SetActive(false);
//             }

//             base.Show();
//             PlayAppearance();
//             _confetti?.Play();
//             _noThanksDelay = DOVirtual.DelayedCall(NoThanksDelay, ShowNoThanks, true);
//         }

//         private void PlayAppearance()
//         {
//             _canvasGroup.alpha = 0f;
//             _appearSequence = DOTween.Sequence().SetUpdate(true);
//             _appearSequence.Append(_canvasGroup.DOFade(1f, _appearDuration * 0.5f));
//             if (_emojiImage != null)
//             {
//                 _emojiImage.transform.localScale = _emojiScale * 0.25f;
//                 _appearSequence.Insert(0f, _emojiImage.transform.DOScale(_emojiScale, _emojiDuration).SetEase(_emojiEase));
//             }
//             if (_perfectImage != null)
//             {
//                 _perfectImage.transform.localScale = _perfectScale * 0.25f;
//                 _appearSequence.Insert(0.1f, _perfectImage.transform.DOScale(_perfectScale, _perfectDuration).SetEase(_perfectEase));
//             }
//         }

//         private static void SetRandomSprite(Image image, List<Sprite> sprites)
//         {
//             if (image == null || sprites == null || sprites.Count == 0)
//                 return;

//             Sprite sprite = sprites[Random.Range(0, sprites.Count)];
//             if (sprite != null) image.sprite = sprite;
//         }

//         private void ShowNoThanks()
//         {
//             if (_rewardClaimed || _noThanksButton == null)
//                 return;

//             _noThanksButton.gameObject.SetActive(true);
//             _noThanksButton.Enable();
//         }

//         private void ClaimTripleReward()
//         {
//             ClaimReward(_reward * Mathf.Max(1, _multiplier), _tripleRewardOrigin);
//         }

//         private void ClaimStandardReward()
//         {
//             ClaimReward(_reward, _rewardOrigin);
//         }

//         private void ClaimReward(float amount, RectTransform origin)
//         {
//             if (_rewardClaimed)
//                 return;

//             _rewardClaimed = true;
//             _noThanksDelay?.Kill();
//             _tripleRewardButton?.Disable();
//             _noThanksButton?.Disable();

//             // Transfer grants and saves the coins, then completes after every visual arrives.
//             _rewardFlyService.Transfer(CounterType.Coins, amount,
//                 origin != null ? origin : (RectTransform)transform,
//                 onComplete: OnRewardsClaimed);
//         }

//         protected override void OnRewardsClaimed()
//         {
//             if (this == null || _transitionStarted)
//                 return;

//             _transitionStarted = true;
//             _levelService.IncrementLevelIndex();
//             base.OnRewardsClaimed();
//         }

//         private void OnEnable()
//         {
//             if (_tripleRewardButton != null) _tripleRewardButton.OnClicked += ClaimTripleReward;
//             if (_noThanksButton != null) _noThanksButton.OnClicked += ClaimStandardReward;
//         }

//         private void OnDisable()
//         {
//             if (_tripleRewardButton != null) _tripleRewardButton.OnClicked -= ClaimTripleReward;
//             if (_noThanksButton != null) _noThanksButton.OnClicked -= ClaimStandardReward;
//             StopAppearance();
//             StopConfetti();
//         }

//         private void StopConfetti()
//         {
//             if (_confetti == null)
//                 return;

//             _confetti.Stop();
//             _confetti.Clear();
//         }

//         private void StopAppearance()
//         {
//             _appearSequence?.Kill();
//             _noThanksDelay?.Kill();
//             if (_canvasGroup != null) _canvasGroup.alpha = 1f;
//             if (_emojiImage != null) _emojiImage.transform.localScale = _emojiScale;
//             if (_perfectImage != null) _perfectImage.transform.localScale = _perfectScale;
//         }
//     }
// }
