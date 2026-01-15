using Aftertime.SecretSome;
using UnityEngine;
using UnityEngine.UI;

namespace Waving.BlackSpin.UI
{
    public class ItemUseConfirmView : View
    {
        public Button ConfirmButton => _confirmButton;
        public Button NoButton => _noButton;
        public Button CloseButton => _closeButton;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private Button _closeButton;
    }
   
}