using System;
using Aftertime.SecretSome.UI.Layout;
using Aftertime.StorylineEngine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Waving.BlackSpin.UI
{
    public class ScreenLayout : LayoutBase
    {
        private ScreenLayoutView _screenLayoutView;
        [SerializeField] private Image _diskImage;
        [SerializeField] private Image _battleSlot;
        [SerializeField] private Image _shopSlot;
        [SerializeField] private Vector2 _battlePos;
        [SerializeField] private Vector2 _shopPos;
        
        protected override void Awake()
        {
            base.Awake();
            LayoutManager.Instance.RegisterLayout(this);
            _screenLayoutView = _view as ScreenLayoutView;
        }

        public void ShowBattlePage()
        {
            if (_screenLayoutView.BattlePage.IsOn == false)
            {
                _diskImage.rectTransform.DOKill();
                _diskImage.DOKill();
                _diskImage.rectTransform.DOAnchorPosY(_shopPos.y + 40, 0.5f);
                _diskImage.DOFade(0,0.5f).onComplete+= () =>
                {
                    _diskImage.rectTransform.anchoredPosition = new Vector2(_battlePos.x,_battlePos.y + 40); 
                    _diskImage.rectTransform.DOAnchorPosY(_battlePos.y, 0.5f);
                    _diskImage.DOFade(1, 0.5f);
                };
            }
            _screenLayoutView.BattlePage.Show(true);
            SoundManager.Instance.PlayBGM("Battle",0.2f);
        }

        public void HideBattlePage()
        {
            _screenLayoutView.BattlePage.Hide(true);
            SoundManager.Instance.StopAllBGM(0.2f);
        }

        public void ShowShopPage()
        {
            if (_screenLayoutView.ShopPage.IsOn == false)
            {
                _diskImage.rectTransform.DOKill();
                _diskImage.DOKill();
                _diskImage.rectTransform.DOAnchorPosY(_battlePos.y + 40, 0.5f);
                _diskImage.DOFade(0,0.5f).onComplete+= () =>
                {
                    _diskImage.rectTransform.anchoredPosition = new Vector2(_shopPos.x,_shopPos.y + 40); 
                    _diskImage.rectTransform.DOAnchorPosY(_shopPos.y, 0.5f);
                    _diskImage.DOFade(1, 0.5f);
                };
            }
            _screenLayoutView.ShopPage.Show(true);
            SoundManager.Instance.PlayBGM("Shop",0.2f);
        }

        public void HideShopPage()
        {
            _screenLayoutView.ShopPage.Hide(true);
            SoundManager.Instance.StopBGM(0.2f);
        }
    }   
}
