using System;
using Aftertime.SecretSome.UI;
using Aftertime.SecretSome.UI.Popup;
using UnityEngine;
using UnityEngine.UI;

namespace Waving.BlackSpin.UI
{
    public class ReGamePopup : PopupBase
    {
        public Button _closeButton;

        private void Start()
        {
            _closeButton.onClick.AddListener(() => PopupManager.Instance.Hide<ReGamePopup>());
        }
    }
   
}