using System.Collections.Generic;
using System.Linq;
using Aftertime.StorylineEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Waving.BlackSpin.Shop;

namespace Waving.BlackSpin.UI
{
    public class DescPanel : SingletonMonoBehaviour<DescPanel>
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private List<ItemData> _items;
        
        public void SetMessage(string title, string message)
        {
            _titleText.text = title;
            _messageText.text = message;
        }

        public void ResetMessage()
        {
            _titleText.text = "";
            _messageText.text = "";
        }
        
        public async void ShowItemDesc(string itemName)
        {
            ItemData itemData = _items.FirstOrDefault(item => item.ItemName == itemName);
            if (itemData == null)
                return;

            string desc = itemData.Desc;
            
            _titleText.text = itemName;
            _messageText.text = desc;

            string originMessage = desc;
            await UniTask.Delay(5000);
            if(_messageText.text == originMessage)
                ResetMessage();
        }
    }
   
}