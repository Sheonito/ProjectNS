using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Percent111.ProjectNS.Battle
{
    // 전투 관리자 (Player/Enemy 생성, StageManager 관리)
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

        [Header("UI")]
        [SerializeField] private StageUI _stageUI;

        private PlayerUnit _player;
        private PlayerDataProvider _playerData;
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;
        private StageManager _stageManager;
        private Transform _poolParent;
        private Transform _projectilePoolParent;

        private void OnEnable()
        {
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<GameRestartEvent>(OnGameRestart);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<GameRestartEvent>(OnGameRestart);
        }

        // 초기화
        public async UniTask Initialize()
        {
            CreatePoolParents();
            SpawnPlayer();
            InitializeProjectilePool();
            InitializeEnemyPool();
            InitializeStageManager();
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

            _stageManager.OnStageStarted += OnStageStarted;
            _stageManager.OnStageCleared += OnStageCleared;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

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

        // 스테이지 시작 이벤트 핸들러
        private void OnStageStarted(int stageNumber)
        {
            Debug.Log($"BattleManager: Stage {stageNumber} started");
        }

        // 스테이지 클리어 이벤트 핸들러
        private void OnStageCleared(int stageNumber)
        {
            Debug.Log($"BattleManager: Stage {stageNumber} cleared");
        }

        // 모든 스테이지 클리어 이벤트 핸들러
        private void OnAllStagesCleared()
        {
            Debug.Log("BattleManager: All stages cleared!");
        }

        // 게임 오버 이벤트 핸들러
        private void OnGameOver(GameOverEvent evt)
        {
            Debug.Log("BattleManager: Game Over!");

            // 게임 오버 팝업 표시
            PopupManager.Instance?.Push<GameOverPopup>();
        }

        // 게임 재시작 이벤트 핸들러
        private void OnGameRestart(GameRestartEvent evt)
        {
            Debug.Log("BattleManager: Game Restart!");

            // 현재 씬 재로드
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }

        // 플레이어 Transform 반환
        public Transform GetPlayerTransform()
        {
            return _player?.transform;
        }

        // 플레이어 반환
        public PlayerUnit GetPlayer()
        {
            return _player;
        }

        // StageManager 반환
        public StageManager GetStageManager()
        {
            return _stageManager;
        }

        // 정리
        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.UnsubscribeEvents();
                _stageManager.OnStageStarted -= OnStageStarted;
                _stageManager.OnStageCleared -= OnStageCleared;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }

            _enemyPool.ClearAll();
            _projectilePool.ClearAll();
            DIResolver.UnregisterInstance<ProjectilePool>();
        }
    }
}
