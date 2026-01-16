using System.Collections.Generic;
using Aftertime.SecretSome.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Waving.BlackSpin.Battle;
using Waving.BlackSpin.DI;
using Waving.BlackSpin.Shop;
using Waving.BlackSpin.Slot;
using Waving.BlackSpin.UI;

namespace Waving.Di
{
    public class DIInstaller : MonoBehaviour
    {
        [SerializeField] private Deck _playerDeck;
        [SerializeField] private Deck _enemyDeck;
        [SerializeField] private TextMeshProUGUI _playerSumText;
        [SerializeField] private TextMeshProUGUI _enemySumText;
        [SerializeField] private TextMeshProUGUI _noticeText;
        [SerializeField] private ExtendedButton _stayButton;
        [SerializeField] private ExtendedButton _hitButton;
        [SerializeField] private Player _player;
        [SerializeField] private Enemy _enemy;
        [SerializeField] private TotalGage _playerTotalGage;
        [SerializeField] private TotalGage _enemyTotalGage;
        [SerializeField] private ItemInventory _itemInventory;
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private Image _playerHPFill;
        [SerializeField] private Image _playerShieldFill;
        [SerializeField] private TextMeshProUGUI _coinTitleText;
        [SerializeField] private TextMeshProUGUI _estimatedDamageText;
        [SerializeField] private RectTransform _battleCanvas;
        [SerializeField] private TextMeshProUGUI _estimatedFactorText;
        [SerializeField] private Image _resultImage;
        
        public void Install()
        {
            ShowdownContainer showdownContainer = new ShowdownContainer(0, 0, _hitButton, _stayButton, _player, _enemy,
                _playerTotalGage, _playerHPFill, _playerShieldFill, _coinTitleText, _battleCanvas,_resultImage);
            new BattleStartContainer(_player, _enemy);
            new SlotGameRuleContainer(_playerDeck, _enemyDeck, _playerSumText, _enemySumText, _noticeText,
                _estimatedDamageText, _stayButton,
                _hitButton, showdownContainer, _player, _enemy, _playerTotalGage, _enemyTotalGage, _coinText,
                _estimatedFactorText);
            new PlayerTurnContainer(_playerDeck, _stayButton, _hitButton);
            new EnemyTurnContainer(_enemyDeck);
            new PlayerAssetContainer(_itemInventory, _coinText);
            new SpecialCardRuleContainer(_player, _playerDeck, _enemyDeck);
            new ActiveCardRuleContainer(_player, _enemy, _playerDeck, _enemyDeck);
            new ShopItemContainer(_player);
        }
    }
}