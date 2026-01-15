using Aftertime.SecretSome.UI.Layout;
using Aftertime.SecretSome.UI.Popup;
using Aftertime.StorylineEngine;
using Cysharp.Threading.Tasks;
using StateMachine.Runtime;
using TMPro;
using UnityEngine;
using Waving.BlackSpin.Battle;
using Waving.BlackSpin.Event;
using Waving.BlackSpin.SaveLoad;
using Waving.BlackSpin.Slot;
using Waving.BlackSpin.UI;
using Waving.Common.Event;
using Waving.Di;
using Waving.UI;

namespace Waving.Scene
{
    public class InGameSceneEntry : ISceneEntry
    {
        private BattleStateMachine _battleStateMachine;
        private SlotGameRule _slotGameRule;
        private DIInstaller _diInstaller;

        private TextMeshProUGUI _stageText;
        public static int _round = 1;

        public void OnEnter()
        {
            InstallDI();
            Init();
            RegisterEvents();
            SoundManager.Instance.PlayBGM("Battle",0.5f);
        }

        public void OnExit()
        {
            SoundManager.Instance.StopBGM(0.2f);
        }

        private void InstallDI()
        {
            if (_diInstaller == null)
                _diInstaller = Object.FindFirstObjectByType<DIInstaller>();
            _diInstaller.Install();
            DIContainerBase.TryInjectAll(PlayerAsset.Instance);
        }

        private void Init()
        {
            _round = 1;
            
            _slotGameRule = new SlotGameRule();
            _slotGameRule.Init();

            _battleStateMachine = new BattleStateMachine();
            _battleStateMachine.Init(null);
            
            ScreenLayout screenLayout = LayoutManager.Instance.GetLayout<ScreenLayout>();
            screenLayout.ShowBattlePage();
        }

        private void RegisterEvents()
        {
            EventBus.Subscribe<NextRoundEvent>(NextRound);
        }

        private void NextRound(NextRoundEvent nextRoundEvent)
        {
            _round++;
            ScreenLayout screenLayout = LayoutManager.Instance.GetLayout<ScreenLayout>();
            screenLayout.ShowBattlePage();
            screenLayout.HideShopPage();

            Enemy enemy = Object.FindFirstObjectByType<Enemy>();
            enemy.LevelUp();
            StartGame();
        }

        public void ReGame()
        {
            _round = 1;
            StartGame();
        }

        public void StartGame()
        {
            const int LastRound = 5;
            if (_round > LastRound)
            {
                SceneEntryManager.Instance.ChangeScene(GameScene.Title);
                return;
            }
            
            PopupManager.Instance.Hide<ReGamePopup>();
            ScreenLayout screenLayout = LayoutManager.Instance.GetLayout<ScreenLayout>();
            screenLayout.HideShopPage();
            screenLayout.ShowBattlePage();

            _slotGameRule.Init();

            _battleStateMachine.ChangeState(BattleContentsState.BattleStart);
            _battleStateMachine.ChangeState(BattleContentsState.EnemyTurn);

            if (_stageText == null)
                _stageText = GameObject.Find("Text_Stage").GetComponent<TextMeshProUGUI>();

            _stageText.text = $"Stage 1-{_round}";
        }
    }
}