using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Event;
using UnityEngine;
namespace Percent111.ProjectNS.Scene
{
    public class InGameSceneEntry : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private BattleManager _battleManager;
        [SerializeField] private DIInstaller _diInstaller;

        public async void OnEnter()
        {
            InstallDI();
            Init();
            await _battleManager.Initialize();
            _battleManager.StartBattle();
        }

        public void OnExit()
        {
        }

        private void InstallDI()
        {
            if (_diInstaller == null)
                return;
            
            _diInstaller.Install();
        }

        private void Init()
        {
        }
    }
}
