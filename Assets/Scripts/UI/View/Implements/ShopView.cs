using Aftertime.SecretSome;
using Aftertime.SecretSome.UI;
using UnityEngine;

namespace Waving.BlackSpin.UI
{
    public class ShopView : PageView
    {
        public ExtendedButton RerollButton => _rerollButton;
        public ExtendedButton NextRoundButton => _nextRoundButton;

        [SerializeField] private ExtendedButton _rerollButton;
        [SerializeField] private ExtendedButton _nextRoundButton;
    }
}