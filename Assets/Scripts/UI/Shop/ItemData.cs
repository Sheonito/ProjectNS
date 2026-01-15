using UnityEngine;
using UnityEngine.Serialization;
using Waving.BlackSpin.Slot;
using Waving.BlackSpin.TestEditor;
using Waving.BlackSpin.UI;
using Waving.TestEditor;

namespace Waving.BlackSpin.Shop
{
    [CreateAssetMenu(
        fileName = "ItemData",
        menuName = "Waving/BlackSpin/Shop/ItemData"
    )]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField] private ItemType itemType;
        [SerializeField] private TestElementType priceType;
        [SerializeField] private TestElementType priceFactorType;
        [SerializeField] private SlotSymbol slotSymbol;
        [TextArea]
        [SerializeField] private string desc;

        public string ItemName => itemName;
        public string Desc => desc;
        public ItemType ItemType => itemType;
        public SlotSymbol SlotSymbol => slotSymbol;
        public Sprite Sprite => ShopItem.GetSprite(itemName);

        public int GetPrice()
        {
            return TestEditorPopup.Instance.GetElementValue<int>(priceType);
        }

        public float GetPriceFactor()
        {
            return TestEditorPopup.Instance.GetElementValue<float>(priceFactorType);
        }
    }   
}