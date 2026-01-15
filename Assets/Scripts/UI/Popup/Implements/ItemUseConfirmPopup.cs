using System;
using Aftertime.SecretSome.UI.Popup;

namespace Waving.BlackSpin.UI
{
    public class ItemUseConfirmPopup : PopupBase
    {
        public event Action<string> onClickConfirm = delegate { };
        public string ItemName { get; private set; }
        private ItemUseConfirmView _itemUseConfirmView;

        public void SetItemName(string itemName)
        {
            ItemName = itemName;
        }

        protected override void Awake()
        {
            base.Awake();
            _itemUseConfirmView = _view as ItemUseConfirmView;
        }

        private void Start()
        {
            _itemUseConfirmView.CloseButton.onClick.AddListener(() => PopupManager.Instance.Hide<ItemUseConfirmPopup>());
            _itemUseConfirmView.ConfirmButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.Hide<ItemUseConfirmPopup>();
                onClickConfirm(ItemName);
            });
            _itemUseConfirmView.NoButton.onClick.AddListener(() => PopupManager.Instance.Hide<ItemUseConfirmPopup>());
        }
    }

}
