using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Directing;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.Spawner;
using UnityEngine;

namespace Percent111.ProjectNS.Scene
{
    public class InGameSceneEntry : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private BattleManager _battleManager;
        [SerializeField] private BattleDIInstaller _battleDIInstaller;

        // Pool 관련
        private Transform _poolParent;
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;

        // Spawner 관련
        private ItemSpawner _itemSpawner;
        private EffectSpawner _effectSpawner;

        private void OnEnable()
        {
            EventBus.Subscribe<GameRestartEvent>(OnGameRestart);
            EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameRestartEvent>(OnGameRestart);
            EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        }

        public async void OnEnter()
        {
            InstallDI();
            InitializePoolsAndSpawners();
            await _battleManager.Initialize();
            _battleManager.StartBattle();
        }

        public void OnExit()
        {
            ClearPoolsAndSpawners();
        }

        private void InstallDI()
        {
            _battleDIInstaller.Install();
        }

        // Pool 및 Spawner 초기화
        private void InitializePoolsAndSpawners()
        {
            CreatePoolParent();
            InitializeProjectilePool();
            InitializeItemSpawner();
            InitializeEffectSpawner();

            // DirectingManager에 EffectSpawner 설정
            DirectingManager.Instance.SetEffectSpawner(_effectSpawner);
        }

        // Pool 부모 오브젝트 생성
        private void CreatePoolParent()
        {
            GameObject poolParentObj = new GameObject("PoolParent");
            poolParentObj.transform.SetParent(transform);
            _poolParent = poolParentObj.transform;
        }

        // Projectile Pool 초기화
        private void InitializeProjectilePool()
        {
            BattleManagerSettings battleSettings = DIResolver.Resolve<BattleManagerSettings>();
            Transform projectilePoolParent = CreateSubPool("ProjectilePool");
            _projectilePool = new ProjectilePool(
                battleSettings.projectilePrefab,
                projectilePoolParent,
                battleSettings.projectilePreLoadCount
            );
            DIResolver.RegisterInstance(_projectilePool);
        }

        // 플레이어 생성 완료 이벤트 핸들러 - EnemyPool 초기화
        private void OnPlayerSpawned(PlayerSpawnedEvent evt)
        {
            InitializeEnemyPool(evt.PlayerData);

            // EnemyPool 초기화 완료 이벤트 발행
            EventBus.Publish(this, new EnemyPoolInitializedEvent(_enemyPool));
        }

        // Enemy Pool 초기화
        private void InitializeEnemyPool(PlayerDataProvider playerData)
        {
            BattleManagerSettings battleSettings = DIResolver.Resolve<BattleManagerSettings>();
            Transform enemyPoolParent = CreateSubPool("EnemyPool");
            _enemyPool = new EnemyPool(enemyPoolParent, playerData);

            _enemyPool.RegisterPrefab(EnemyType.Melee, battleSettings.meleeEnemyPrefab);
            _enemyPool.PreLoad(EnemyType.Melee, battleSettings.enemyPreLoadCount);

            _enemyPool.RegisterPrefab(EnemyType.Ranged, battleSettings.rangedEnemyPrefab);
            _enemyPool.PreLoad(EnemyType.Ranged, battleSettings.enemyPreLoadCount);

            DIResolver.RegisterInstance(_enemyPool);
        }

        // Item Spawner 초기화
        private void InitializeItemSpawner()
        {
            ItemSpawnerSettings itemSettings = DIResolver.Resolve<ItemSpawnerSettings>();
            _itemSpawner = new ItemSpawner(
                itemSettings.shieldItemPrefab,
                itemSettings.dropChance,
                itemSettings.preLoadCount
            );
            _itemSpawner.Initialize(_poolParent);
        }

        // Effect Spawner 초기화
        private void InitializeEffectSpawner()
        {
            _effectSpawner = new EffectSpawner();
            _effectSpawner.Initialize(_poolParent);
        }

        // 하위 Pool 생성
        private Transform CreateSubPool(string name)
        {
            GameObject subPoolObj = new GameObject(name);
            subPoolObj.transform.SetParent(_poolParent);
            return subPoolObj.transform;
        }

        // 게임 재시작 이벤트 핸들러 - 전체 재시작 흐름 관리
        private void OnGameRestart(GameRestartEvent evt)
        {
            RestartGameAsync().Forget();
        }

        // 게임 재시작 (비동기)
        private async UniTaskVoid RestartGameAsync()
        {
            // 1. BattleManager 데이터 정리 (StageManager, Player 등)
            _battleManager.ResetBattleData();

            // 2. Pool/Spawner 정리
            ClearPoolsAndSpawners();

            // 3. Pool/Spawner 재초기화
            InitializePoolsAndSpawners();

            // 4. BattleManager 재초기화 및 전투 시작
            await _battleManager.Initialize();
            _battleManager.StartBattle();
        }

        // Pool 및 Spawner 정리
        private void ClearPoolsAndSpawners()
        {
            // EnemyPool 정리
            if (_enemyPool != null)
            {
                _enemyPool.ClearAll();
                DIResolver.UnregisterInstance<EnemyPool>();
                _enemyPool = null;
            }

            // ProjectilePool 정리
            if (_projectilePool != null)
            {
                _projectilePool.ClearAll();
                DIResolver.UnregisterInstance<ProjectilePool>();
                _projectilePool = null;
            }

            // ItemSpawner 정리
            if (_itemSpawner != null)
            {
                _itemSpawner.Dispose();
                _itemSpawner = null;
            }

            // EffectSpawner 정리
            if (_effectSpawner != null)
            {
                _effectSpawner.Dispose();
                _effectSpawner.ClearAll();
                _effectSpawner = null;
            }

            // Pool 부모 오브젝트 제거
            if (_poolParent != null)
            {
                Destroy(_poolParent.gameObject);
                _poolParent = null;
            }
        }

        private void OnDestroy()
        {
            ClearPoolsAndSpawners();
        }
    }
}
