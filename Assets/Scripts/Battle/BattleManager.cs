using System.Collections.Generic;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Player;
using UnityEngine;

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

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> _enemySpawnPoints;

        private PlayerUnit _player;
        private EnemyPool _enemyPool;
        private StageManager _stageManager;
        private Transform _poolParent;

        private void Awake()
        {
            Initialize();
        }

        // 초기화
        private void Initialize()
        {
            CreatePoolParent();
            SpawnPlayer();
            InitializeEnemyPool();
            InitializeStageManager();
            StartBattle();
        }

        // 풀 부모 오브젝트 생성
        private void CreatePoolParent()
        {
            GameObject poolObj = new GameObject("EnemyPool");
            poolObj.transform.SetParent(transform);
            _poolParent = poolObj.transform;
        }

        // 플레이어 생성
        private void SpawnPlayer()
        {
            Vector3 spawnPos = _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;
            GameObject playerObj = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);
            _player = playerObj.GetComponent<PlayerUnit>();
        }

        // 적 풀 초기화
        private void InitializeEnemyPool()
        {
            _enemyPool = new EnemyPool(_poolParent, _player.transform);

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
        }

        // 전투 시작
        private void StartBattle()
        {
            _stageManager.StartStage(1);
        }

        private void Update()
        {
            _stageManager?.UpdateEnemySeparation();
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

            _enemyPool?.ClearAll();
        }
    }
}
