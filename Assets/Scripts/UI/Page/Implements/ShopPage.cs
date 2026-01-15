using System;
using System.Collections.Generic;
using System.Linq;
using Aftertime.SecretSome.Common;
using Aftertime.SecretSome.UI;
using Aftertime.SecretSome.UI.Page;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Waving.BlackSpin.Event;
using Waving.BlackSpin.SaveLoad;
using Waving.BlackSpin.Shop;
using Waving.BlackSpin.TestEditor;
using Waving.Common.Event;
using Waving.TestEditor;
using Random = UnityEngine.Random;

namespace Waving.BlackSpin.UI
{
    public class ShopPage : PageBase
    {
        [SerializeField] private TextMeshProUGUI _rerollPriceText;
        [SerializeField] private List<ItemData> _itemDataList;
        [SerializeField] private List<ShopItem> _passiveItems;
        [SerializeField] private List<ShopItem> _activeItems;
        [SerializeField] private List<ShopItem> _cardItems;
        [SerializeField] private int _displayPassiveItemCount = 4;
        [SerializeField] private int _displayActiveItemCount = 3;
        [SerializeField] private int _displayCardItemCount = 3;
        [SerializeField] private List<CanvasGroup> _sideGroups;

        private ShopView _shopView;

        protected override void Awake()
        {
            base.Awake();
            _shopView = _view as ShopView;
            RegisterEvents();
            UpdateExecutor.onUpdate += () =>
            {
                // int rerollCost = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.RerollCoinCount);
                // _rerollPriceText.text = $"${rerollCost}";
            };
        }

        private void RegisterEvents()
        {
            ExtendedButton rerollButton = _shopView.RerollButton;
            rerollButton.onClick.AddListener(ReRoll);

            ExtendedButton nextRoundButton = _shopView.NextRoundButton;
            nextRoundButton.onClick.AddListener(MoveNextRound);
        }

        public override void Show(bool hasDuration)
        {
            base.Show(hasDuration);
            _sideGroups.ForEach(group => group.DOFade(0,0.5f));
            UpdateItems();
        }

        public override void Hide(bool hasDuration)
        {
            base.Hide(hasDuration);
            _sideGroups.ForEach(group => group.DOFade(1,0.5f));
        }

        private void UpdateItems()
        {
            UpdateItems(_passiveItems,ItemType.Passive,_displayPassiveItemCount);
            UpdateItems(_activeItems,ItemType.Active,_displayActiveItemCount);
            UpdateItems(_cardItems,ItemType.Card,_displayCardItemCount);
        }

        private void UpdateItems(List<ShopItem> items,ItemType itemType,int displayCount)
        {
            List<ItemData> candidateItems;
            if (itemType == ItemType.Active || itemType == ItemType.Card)
            {
                candidateItems = _itemDataList.Where(item => item.ItemType == itemType)
                    .ToList()
                    .FindAll(item => !PlayerAsset.Instance.IsPurchased(item));   
            }
            else
            {
                candidateItems = _itemDataList.Where(item => item.ItemType == itemType)
                    .ToList();
            }

            int candidateCount = candidateItems.Count;

            // 중복되지 않게 랜덤으로 데이터 나열
            List<int> indexPool = new();
            for (int i = 0; i < candidateCount; i++)
                indexPool.Add(i);

            for (int i = 0; i < indexPool.Count; i++)
            {
                int swap = Random.Range(i, indexPool.Count);
                (indexPool[i], indexPool[swap]) = (indexPool[swap], indexPool[i]);
            }

            for (int i = 0; i < displayCount; i++)
            {
                if (candidateItems.Count <= i)
                    return;
                
                ItemData itemData = candidateItems[indexPool[i]];
                items[i].UpdateData(itemData);
            }
        }

        private void ReRoll()
        {
            int rerollCost = TestEditorPopup.Instance.GetElementValue<int>(TestElementType.RerollCoinCount);
            int playerCoin = PlayerAsset.Instance.Coin;
            if (playerCoin < rerollCost)
                return;

            PlayerAsset.Instance.Coin -= rerollCost;
            UpdateItems();
        }

        private void MoveNextRound()
        {
            EventBus.Publish<NextRoundEvent>(this);
        }
    }
}