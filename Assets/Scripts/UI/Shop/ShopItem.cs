using System;
using Aftertime.SecretSome.UI;
using Aftertime.SecretSome.UI.Popup;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Waving.BlackSpin.Battle;
using Waving.BlackSpin.Common;
using Waving.BlackSpin.DI;
using Waving.BlackSpin.SaveLoad;
using Waving.BlackSpin.Shop;
using Waving.BlackSpin.TestEditor;
using Waving.Di;
using Waving.TestEditor;

namespace Waving.BlackSpin.UI
{
    public class ShopItem : DIMono, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private ExtendedButton _buyButton;
        [SerializeField] private Image _itemImage;

        private const float EnterScale = 1.05f;
        private const float ScaleDuration = 0.2f;

        [Inject] private ShopItemContainer _container;
        private ItemData _itemData;

        protected override void Awake()
        {
            base.Awake();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _buyButton.onClick.AddListener(() =>
            {
                int price = _itemData.GetPrice();
                if (PlayerAsset.Instance.Coin >= price)
                {
                    PopupManager.Instance.Push<BuyConfirmPopup>();
                    BuyConfirmPopup buyConfirmPopup = PopupManager.Instance.GetPopup<BuyConfirmPopup>();
                    buyConfirmPopup.onClickConfirm += Buy;
                }
            });
        }

        private void Buy()
        {
            if (_container == null)
                DIContainerBase.TryInjectAll(this);

            int itemLevel = GetItemLevel(_itemData.ItemName);
            bool isPassive = _itemData.ItemType == ItemType.Passive;
            float priceFactor = isPassive ? _itemData.GetPriceFactor() : 1;
            int price = GetPrice(_itemData.GetPrice(), itemLevel, priceFactor);
            PlayerAsset.Instance.Coin -= price;
            PlayerAsset.Instance.AddItem(_itemData);
            gameObject.SetActive(false);
            PopupManager.Instance.Hide<BuyConfirmPopup>();

            if (_itemData.ItemType == ItemType.Passive)
            {
                switch (_itemData.ItemName)
                {
                    case Define.EmergencyKitData:
                        PlayerAsset.emergencyKitLevel++;
                        break;

                    case Define.ShieldUpData:
                        PlayerAsset.shieldUpLevel++;
                        break;

                    case Define.TwinSwordData:
                        PlayerAsset.twinSwordLevel++;
                        break;

                    case Define.CoinUpData:
                        PlayerAsset.CoinUpLevel++;
                        break;

                    case Define.CoinThrowData:
                        PlayerAsset.CoinThrowLevel++;
                        break;

                    case Define.ShieldSlamData:
                        PlayerAsset.ShieldSlamLevel++;
                        break;
                }
            }
        }

        public void UpdateData(ItemData itemData)
        {
            gameObject.SetActive(true);

            _itemData = itemData;
            string itemName = itemData.ItemName;
            float price = itemData.GetPrice();
            Sprite sprite = GetSprite(itemName);
            if (itemData.ItemType == ItemType.Passive)
            {
                float priceFactor = itemData.GetPriceFactor();
                int itemLevel = GetItemLevel(itemName);
                price = GetPrice(price, itemLevel, priceFactor);
                _priceText.color = itemLevel >= 1 ? Color.yellow : _priceText.color;
            }
            else
            {
                price = itemData.GetPrice();
            }

            _priceText.text = "$ " + price;
            _itemImage.sprite = sprite;
        }

        private int GetPrice(float price, int itemLevel, float priceFactor)
        {
            itemLevel = Mathf.Clamp(itemLevel, 1, int.MaxValue);
            price = _itemData.GetPrice() * (itemLevel * priceFactor);
            int resultPrice = (int)Math.Round(price, MidpointRounding.AwayFromZero);
            return resultPrice;
        }

        private int GetItemLevel(string itemName)
        {
            switch (itemName)
            {
                case Define.EmergencyKitData:
                    return PlayerAsset.emergencyKitLevel;

                case Define.ShieldUpData:
                    return PlayerAsset.shieldUpLevel;

                case Define.TwinSwordData:
                    return PlayerAsset.twinSwordLevel;

                case Define.CoinUpData:
                    return PlayerAsset.CoinUpLevel;

                case Define.CoinThrowData:
                    return PlayerAsset.CoinThrowLevel;

                case Define.ShieldSlamData:
                    return PlayerAsset.ShieldSlamLevel;

                default:
                    return 1;
            }
        }

        public static Sprite GetSprite(string itemName)
        {
            switch (itemName)
            {
                case Define.WickedFlowerData:
                    return Resources.Load<Sprite>(Define.WickedFlowerSprite);

                case Define.BonFireData:
                    return Resources.Load<Sprite>(Define.BonFireSprite);

                case Define.ImposterData:
                    return Resources.Load<Sprite>(Define.ImposterSprite);

                case Define.TrapCardData:
                    return Resources.Load<Sprite>(Define.TrapCardSprite);

                case Define.BlueScreenData:
                    return Resources.Load<Sprite>(Define.BlueScreenSprite);

                case Define.JeweledGauntletData:
                    return Resources.Load<Sprite>(Define.JeweledGauntletSprite);

                case Define.MintChocoData:
                    return Resources.Load<Sprite>(Define.MintChocoSprite);

                case Define.MinecraftData:
                    return Resources.Load<Sprite>(Define.MinecraftSprite);

                case Define.EmergencyKitData:
                    return Resources.Load<Sprite>(Define.EmergencyKitSprite);

                case Define.ShieldUpData:
                    return Resources.Load<Sprite>(Define.ShieldUpSprite);

                case Define.TwinSwordData:
                    return Resources.Load<Sprite>(Define.TwinSwordSprite);

                case Define.CoinUpData:
                    return Resources.Load<Sprite>(Define.CoinUpSprite);

                case Define.X2Data:
                    return Resources.Load<Sprite>(Define.X2Sprite);

                case Define.TreasureBoxData:
                    return Resources.Load<Sprite>(Define.TreasureBoxSprite);

                case Define.CoinThrowData:
                    return Resources.Load<Sprite>(Define.CoinThrowSprite);

                case Define.CrystalBoxData:
                    return Resources.Load<Sprite>(Define.CrystalBoxSprite);

                case Define.CupidArrowData:
                    return Resources.Load<Sprite>(Define.CupidArrowSprite);

                case Define.ForgeData:
                    return Resources.Load<Sprite>(Define.ForgeSprite);

                case Define.FourLeafCloverData:
                    return Resources.Load<Sprite>(Define.FourLeafCloverSprite);

                case Define.ShieldSlamData:
                    return Resources.Load<Sprite>(Define.ShieldSlamSprite);

                case Define.ChangeHeartData:
                    return Resources.Load<Sprite>(Define.ChangeHeartSprite);

                case Define.ChangeDiamondData:
                    return Resources.Load<Sprite>(Define.ChangeDiamondSprite);

                case Define.ChangeSpadeData:
                    return Resources.Load<Sprite>(Define.ChangeSpadeSprite);

                case Define.ChangeCloverData:
                    return Resources.Load<Sprite>(Define.ChangeCloverSprite);

                default:
                    return null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_container == null)
                DIContainerBase.TryInjectAll(this);

            transform.DOScale(EnterScale, ScaleDuration);
            string itemName = _itemData.ItemName;
            string desc = _itemData.Desc;
            string additionalDesc = null;
            int value = 0;
            float factor = 0;
            if (_itemData.ItemType == ItemType.Passive)
            {
                switch (_itemData.ItemName)
                {
                    case Define.EmergencyKitData:
                        value = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.EmergencyKitValue);
                        factor = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.EmergencyKitFactor);
                        additionalDesc = $" (다음 레벨: {value * factor * (PlayerAsset.emergencyKitLevel + 1)})";
                        itemName += $" Level{PlayerAsset.emergencyKitLevel + 1}";
                        break;

                    case Define.ShieldUpData:
                        value = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.ShieldUpValue);
                        factor = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.ShieldUpFactor);
                        additionalDesc = $" (다음 레벨: {value * factor * (PlayerAsset.shieldUpLevel + 1 )})";
                        itemName += $" Level{PlayerAsset.shieldUpLevel + 1}";
                        break;

                    case Define.TwinSwordData:
                        value = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.TwinSwordValue);
                        factor = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.TwinSwordFactor);
                        additionalDesc = $" (다음 레벨: {value * factor * (PlayerAsset.twinSwordLevel + 1)})";
                        itemName += $" Level{PlayerAsset.twinSwordLevel + 1}";
                        break;

                    case Define.CoinUpData:
                        value = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.CoinUpValue);
                        factor = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.CoinUpFactor);
                        additionalDesc = $" (다음 레벨: {value * factor * (PlayerAsset.CoinUpLevel + 1)})";
                        itemName += $" Level{PlayerAsset.CoinUpLevel + 1}";
                        break;

                    case Define.ShieldSlamData:
                        factor = TestEditorPopup.Instance.GetElementValue<float>(TestElementType.ShieldSlamFactor);
                        int shieldSlamLevel = PlayerAsset.ShieldSlamLevel + 1;
                        additionalDesc = $"\n(다음 레벨 데미지: 쉴드 수치 X {shieldSlamLevel * factor})";
                        itemName += $" Level{shieldSlamLevel}";
                        break;

                    case Define.CoinThrowData:
                        int damage = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.CoinThrowValue);
                        int coinCount = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.CoinThrowFactor);
                        int coinThrowLevel = PlayerAsset.CoinThrowLevel + 1;
                        additionalDesc = $"\n(다음 레벨 데미지: 코인 {coinThrowLevel + coinCount}개 X {damage})";
                        itemName += $" Level{coinThrowLevel}";
                        break;
                }
            }

            desc += additionalDesc;
            DescPanel.Instance.SetMessage(itemName, desc);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one, ScaleDuration);
            DescPanel.Instance.ResetMessage();
        }
    }
}