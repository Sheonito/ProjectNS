using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Directing;
using Percent111.ProjectNS.Effect;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Item;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.Spawner;
using Percent111.ProjectNS.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 전투 관리자 (Player/Enemy 생성, StageManager 관리, Spawner/연출 관리)
    public class BattleManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject _meleeEnemyPrefab;
        [SerializeField] private GameObject _rangedEnemyPrefab;

        [Header("Settings")]
        [SerializeField] private StageSettings _stageSettings;
        [SerializeField] private int _preLoadCount = 10;

        [Header("Projectile")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private int _projectilePreLoadCount = 20;

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> _enemySpawnPoints;

        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("UI")]
        [SerializeField] private StageUI _stageUI;

        [Header("Item Spawner")]
        [SerializeField] private ShieldItem _shieldItemPrefab;
        [SerializeField] private float _shieldDropChance = 0.2f;
        [SerializeField] private int _shieldPreLoadCount = 10;

        [Header("Effect Spawner")]
        [SerializeField] private HitEffect _hitEffectPrefab;
        [SerializeField] private int _hitEffectPreLoadCount = 20;

        [Header("Directing")]
        [SerializeField] private float _hitStopDuration = 0.05f;
        [SerializeField] private float _hitStopTimeScale = 0f;
        [SerializeField] private float _cameraShakeForce = 0.5f;

        private PlayerUnit _player;
        private PlayerDataProvider _playerData;
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;
        private StageManager _stageManager;
        private Transform _poolParent;
        private Transform _projectilePoolParent;

        // Spawner & Directing
        private ItemSpawner _itemSpawner;
        private EffectSpawner _effectSpawner;
        private DirectingManager _directingManager;

        // Static accessor for DirectingManager (used by combat systems)
        public static DirectingManager Directing { get; private set; }

        private void OnEnable()
        {
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<GameClearEvent>(OnGameClear);
            EventBus.Subscribe<GameRestartEvent>(OnGameRestart);
            EventBus.Subscribe<AllStagesClearedEvent>(OnAllStagesCleared);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<GameClearEvent>(OnGameClear);
            EventBus.Unsubscribe<GameRestartEvent>(OnGameRestart);
            EventBus.Unsubscribe<AllStagesClearedEvent>(OnAllStagesCleared);
        }

        // 초기화
        public async UniTask Initialize()
        {
            CreatePoolParents();
            SpawnPlayer();
            InitializeProjectilePool();
            InitializeEnemyPool();
            InitializeStageManager();
            InitializeSpawners();
            InitializeDirectingManager();
        }

        // ItemSpawner & EffectSpawner 초기화
        private void InitializeSpawners()
        {
            // ItemSpawner 초기화
            if (_shieldItemPrefab != null)
            {
                _itemSpawner = new ItemSpawner(_shieldItemPrefab, _shieldDropChance, _shieldPreLoadCount);
                _itemSpawner.Initialize(transform);
            }

            // EffectSpawner 초기화
            if (_hitEffectPrefab != null)
            {
                _effectSpawner = new EffectSpawner(_hitEffectPrefab, _hitEffectPreLoadCount);
                _effectSpawner.Initialize(transform);
            }
        }

        // DirectingManager 초기화
        private void InitializeDirectingManager()
        {
            _directingManager = new DirectingManager(
                _impulseSource,
                _hitStopDuration,
                _hitStopTimeScale,
                _cameraShakeForce
            );

            if (_effectSpawner != null)
            {
                _directingManager.SetEffectSpawner(_effectSpawner);
            }

            // Static accessor 설정
            Directing = _directingManager;
        }

        // 풀 부모 오브젝트 생성
        private void CreatePoolParents()
        {
            GameObject enemyPoolObj = new GameObject("EnemyPool");
            enemyPoolObj.transform.SetParent(transform);
            _poolParent = enemyPoolObj.transform;

            GameObject projectilePoolObj = new GameObject("ProjectilePool");
            projectilePoolObj.transform.SetParent(transform);
            _projectilePoolParent = projectilePoolObj.transform;
        }

        // 투사체 풀 초기화
        private void InitializeProjectilePool()
        {
            if (_projectilePrefab != null)
            {
                _projectilePool = new ProjectilePool(_projectilePrefab, _projectilePoolParent, _projectilePreLoadCount);
                DIResolver.RegisterInstance(_projectilePool);
            }
        }

        // 플레이어 생성
        private void SpawnPlayer()
        {
            Vector3 spawnPos = _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;
            GameObject playerObj = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);
            _player = playerObj.GetComponent<PlayerUnit>();
            _playerData = _player.CreateDataProvider();

            SetCinemachineTarget();
        }

        // Cinemachine 카메라 타겟 설정
        private void SetCinemachineTarget()
        {
            if (_cinemachineCamera != null && _player != null)
            {
                _cinemachineCamera.Follow = _player.transform;
            }
        }

        // 적 풀 초기화
        private void InitializeEnemyPool()
        {
            _enemyPool = new EnemyPool(_poolParent, _playerData);

            if (_meleeEnemyPrefab != null)
            {
                _enemyPool.RegisterPrefab(EnemyType.Melee, _meleeEnemyPrefab);
                _enemyPool.PreLoad(EnemyType.Melee, _preLoadCount);
            }

            if (_rangedEnemyPrefab != null)
            {
                _enemyPool.RegisterPrefab(EnemyType.Ranged, _rangedEnemyPrefab);
                _enemyPool.PreLoad(EnemyType.Ranged, _preLoadCount);
            }
        }

        // StageManager 초기화
        private void InitializeStageManager()
        {
            _stageManager = new StageManager(_stageSettings, _enemyPool, _enemySpawnPoints);
            _stageManager.SubscribeEvents();

            // StageUI 초기화
            if (_stageUI != null)
            {
                _stageUI.Initialize(_stageManager);
            }
        }

        // 전투 시작
        public void StartBattle()
        {
            _stageManager.StartStage(1);
        }

        private void Update()
        {
            if (_stageManager == null) return;

            _stageManager.UpdateTimer();
            _stageManager.UpdateEnemySeparation();
        }

        // 모든 스테이지 클리어 이벤트 핸들러
        private void OnAllStagesCleared(AllStagesClearedEvent evt)
        {
            // 게임 클리어 이벤트 발행
            EventBus.Publish(this, new GameClearEvent());
        }

        // 게임 오버 이벤트 핸들러
        private void OnGameOver(GameOverEvent evt)
        {
            // 게임 오버 팝업 표시
            PopupManager.Instance.Push<GameOverPopup>();
        }

        // 게임 클리어 이벤트 핸들러
        private void OnGameClear(GameClearEvent evt)
        {
            // 게임 클리어 팝업 표시
            PopupManager.Instance?.Push<GameClearPopup>();
        }

        // 게임 재시작 이벤트 핸들러
        private void OnGameRestart(GameRestartEvent evt)
        {
            // 기존 데이터 정리 후 재시작
            RestartBattleAsync().Forget();
        }

        // 전투 재시작 (비동기)
        private async UniTaskVoid RestartBattleAsync()
        {
            // 기존 데이터 정리
            ResetScene();

            // 재초기화 및 전투 시작
            await Initialize();
            StartBattle();
        }

        // 재시작을 위한 정리
        private void ResetScene()
        {
            // StageManager 정리
            if (_stageManager != null)
            {
                _stageManager.UnsubscribeEvents();
                _stageManager = null;
            }

            // 기존 플레이어 제거
            if (_player != null)
            {
                Destroy(_player.gameObject);
                _player = null;
                _playerData = null;
            }

            // 적 풀 정리
            _enemyPool?.ClearAll();
            _enemyPool = null;

            // 투사체 풀 정리
            _projectilePool?.ClearAll();
            DIResolver.UnregisterInstance<ProjectilePool>();
            _projectilePool = null;

            // ItemSpawner 정리
            _itemSpawner?.Dispose();
            _itemSpawner = null;

            // EffectSpawner 정리
            _effectSpawner?.Dispose();
            _effectSpawner = null;

            // DirectingManager 정리
            _directingManager?.Dispose();
            _directingManager = null;
            Directing = null;

            // 풀 부모 오브젝트 제거
            if (_poolParent != null)
            {
                Destroy(_poolParent.gameObject);
                _poolParent = null;
            }

            if (_projectilePoolParent != null)
            {
                Destroy(_projectilePoolParent.gameObject);
                _projectilePoolParent = null;
            }
        }
        
        // 정리
        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.UnsubscribeEvents();
            }

            _enemyPool?.ClearAll();
            _projectilePool?.ClearAll();
            DIResolver.UnregisterInstance<ProjectilePool>();

            _itemSpawner?.Dispose();
            _effectSpawner?.Dispose();
            _directingManager?.Dispose();
            Directing = null;
        }
    }
}
