using Aftertime.SecretSome.UI;
using Aftertime.SecretSome.UI.Layout;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyNamespace;
using UnityEngine;
using UnityEngine.Rendering;
using Waving.Scene;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Waving.BlackSpin.UI
{
    public class TitleLayout : LayoutBase
    {
        private TitleLayoutView _titleLayoutView;
        [SerializeField] private CanvasGroup _glowGroup;
        [SerializeField] private Volume _volume;
        [SerializeField] private Image _blackImage;
        [SerializeField] private Image _diskImage;
        [SerializeField] private CanvasGroup _battleGroup;
        [SerializeField] private CanvasGroup _titleButtonGroup;
        [SerializeField] private Image _overlayImage;
        [SerializeField] private Button _exitButton;

        protected override void Awake()
        {
            base.Awake();
            _titleLayoutView = _view as TitleLayoutView;
        }

        protected override void Start()
        {
            base.Start();
            RegisterEvents();
        }

        public override void Show(bool isShowImmediately = false)
        {
            base.Show(isShowImmediately);
            _overlayImage.color = Color.black;
            _overlayImage.DOFade(0, _fadeDuration);
            _titleLayoutView.StartButton.enabled = true;
        }

        private void RegisterEvents()
        {
            ExtendedButton startButton = _titleLayoutView.StartButton;
            startButton.onClick.AddListener(StartNewGame);
            startButton.onClick.AddListener(() => startButton.enabled = false);
            _exitButton.onClick.AddListener(Application.Quit);
        }

        private async void StartNewGame()
        {
            _volume.profile.TryGet(out Bloom bloom);
            float bloomInDuration = 1f;
            var bloomInTween = DOTween.To(
                () => bloom.intensity.value, // getter
                x => bloom.intensity.value = x, // setter
                10f, // 목표값
                bloomInDuration // 시간
            );

            float diskDuration = 0.3f;
            float canvasGroupDuration = 0.5f;
            
            float bloomOutDuration = 1f;
            var bloomOutTween = DOTween.To(
                () => bloom.intensity.value, // getter
                x => bloom.intensity.value = x, // setter
                0.2f, // 목표값
                bloomInDuration // 시간
            );


            Sequence sequence = DOTween.Sequence();
            sequence.Append(bloomInTween);
            sequence.Join(_glowGroup.DOFade(1, bloomInDuration));
            sequence.AppendInterval(0.1f);
            sequence.Append(_blackImage.DOFade(0, bloomOutDuration));
            sequence.Join(bloomOutTween);
            sequence.Append(_diskImage.DOFade(1, diskDuration));
            sequence.Join(_diskImage.rectTransform.DOAnchorPosY(-688f,diskDuration));
            sequence.AppendInterval(0.1f);
            sequence.Append(_diskImage.rectTransform.DOAnchorPosY(-696f, 0.1f));
            sequence.Join(_titleButtonGroup.DOFade(0, canvasGroupDuration));
            sequence.Append(_battleGroup.DOFade(1, canvasGroupDuration));
            sequence.AppendInterval(0.1f);
            await sequence.ToUniTask();
            
            await SceneEntryManager.Instance.Additive(GameScene.InGame);
            await SceneEntryManager.Instance.Remove(GameScene.Title);
        }
    }
   
}