using System;
using Aftertime.SecretSome.UI.Popup;

namespace Waving.BlackSpin.UI
{
    public class BuyConfirmPopup : PopupBase
    {
        public event Action onClickConfirm = delegate { };
        
        private BuyConfirmView _buyConfirmView;

        protected override void Awake()
        {
            base.Awake();
            _buyConfirmView = _view as BuyConfirmView;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _buyConfirmView.ConfirmButton.onClick.AddListener(() =>
            {
                onClickConfirm();
                onClickConfirm = delegate { };
            });
            _buyConfirmView.CloseButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.Hide<BuyConfirmPopup>();
                onClickConfirm = delegate { };
            });
            _buyConfirmView.NoButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.Hide<BuyConfirmPopup>();
                onClickConfirm = delegate { };
            });
        }
    }
   
}