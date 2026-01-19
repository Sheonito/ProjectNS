using Percent111.ProjectNS.Battle;
using UnityEngine;
using UnityEngine.Serialization;

namespace Percent111.ProjectNS.DI
{
    /// <summary>
    /// 전투 관련 DI 설정 등록 (씬 진입 시 실행)
    /// </summary>
    public class BattleDIInstaller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private BattleManagerSettings _battleManagerSettings;
        [SerializeField] private StageSettings _stageSettings;
        [SerializeField] private ItemSpawnerSettings _itemSpawnerSettings;
        [SerializeField] private EffectSpawnerSettings _effectSpawnerSettings;

        [Header("Containers")]
        [SerializeField] private BattleSceneContainer _sceneContainer;
        [SerializeField] private DirectingContainer _directingContainer;

        public void Install()
        {
            // ScriptableObject 설정 등록
            DIResolver.RegisterInstance(_battleManagerSettings);
            DIResolver.RegisterInstance(_stageSettings);
            DIResolver.RegisterInstance(_directingContainer);
            DIResolver.RegisterInstance(_itemSpawnerSettings);
            DIResolver.RegisterInstance(_effectSpawnerSettings);

            // 씬 컨테이너 등록
            DIResolver.RegisterInstance(_sceneContainer);
        }

        private void OnDestroy()
        {
            // 정리
            DIResolver.UnregisterInstance<BattleManagerSettings>();
            DIResolver.UnregisterInstance<StageSettings>();
            DIResolver.UnregisterInstance<DirectingContainer>();
            DIResolver.UnregisterInstance<ItemSpawnerSettings>();
            DIResolver.UnregisterInstance<EffectSpawnerSettings>();
            DIResolver.UnregisterInstance<BattleSceneContainer>();
        }
    }
}
