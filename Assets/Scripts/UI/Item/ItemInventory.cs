using UnityEngine;
using Waving.BlackSpin.Shop;

namespace Waving.BlackSpin.UI
{
    public class ItemInventory : MonoBehaviour
    {
        [SerializeField] private RectTransform _itemParent;
        [SerializeField] private BattleItem _battleItemPrefab;

        public void AddItem(ItemData itemData)
        {
            BattleItem item = Instantiate(_battleItemPrefab, _itemParent);
            item.UpdateData(itemData);
        }
    }
   
}