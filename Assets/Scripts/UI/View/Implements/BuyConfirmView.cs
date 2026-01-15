using Aftertime.SecretSome;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Waving.BlackSpin.UI
{
    public class BuyConfirmView : View
    {
        public Button ConfirmButton => _confirmButton;
        public Button CloseButton => _closeButton;
        public Button NoButton => _noButton;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _noButton;
    }   
}
