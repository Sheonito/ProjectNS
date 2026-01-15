using System;
using Aftertime.SecretSome.UI;
using Aftertime.SecretSome.UI.Popup;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Waving.BlackSpin.Event;
using Waving.BlackSpin.Shop;
using Waving.Common.Event;

namespace Waving.BlackSpin.UI
{
    public class BattleItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private ExtendedButton _button;
        private ItemData _itemData;

        public void UpdateData(ItemData data)
        {
            string itemName = data.ItemName;
            _itemData = data;
            _image.sprite = ShopItem.GetSprite(itemName);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            string itemName = _itemData.ItemName;
            string desc = _itemData.Desc;
            DescPanel.Instance.SetMessage(itemName, desc);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DescPanel.Instance.ResetMessage();
        }

        private void Awake()
        {
            EventBus.Subscribe<NextRoundEvent>((_) => ResetItem());
            ItemUseConfirmPopup itemUseConfirmPopup = PopupManager.Instance.GetPopup<ItemUseConfirmPopup>();
            _button.onClick.AddListener(() =>
            {
                PopupManager.Instance.Push<ItemUseConfirmPopup>();
                itemUseConfirmPopup.SetItemName(_itemData.ItemName);
            });
            itemUseConfirmPopup.onClickConfirm += (itemName) =>
            {
                if (itemName == _itemData.ItemName)
                    _button.interactable = false;
            };
        }

        private void ResetItem()
        {
            _button.interactable = true;
        }
    }
}