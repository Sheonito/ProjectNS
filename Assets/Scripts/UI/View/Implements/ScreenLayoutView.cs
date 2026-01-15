using Aftertime.SecretSome;
using UnityEngine;

namespace Waving.BlackSpin.UI
{
    public class ScreenLayoutView : View
    {
        public BattlePage BattlePage => _battlePage;
        public ShopPage ShopPage => _shopPage;
        
        [SerializeField] private BattlePage _battlePage;
        [SerializeField] private ShopPage _shopPage;
    }
   
}