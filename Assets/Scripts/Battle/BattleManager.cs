using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Directing;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.UI;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    public class BattleManager : MonoBehaviour
    {
        // DI Settings
        private BattleManagerSettings _battleSettings;
        private StageSettings _stageSettings;
        private BattleSceneContainer _sceneContainer;

        // Runtime instances
        private PlayerUnit _player;
        private PlayerDataProvider _playerData;
        private StageManager _stageManager;

        private void OnEnable()
        {
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<AllStagesClearedEvent>(OnAllStagesCleared);
            EventBus.Subscribe<EnemyPoolInitializedEvent>(OnEnemyPoolInitialized);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<AllStagesClearedEvent>(OnAllStagesCleared);
            EventBus.Unsubscribe<EnemyPoolInitializedEvent>(OnEnemyPoolInitialized);
        }

        // 초기화
        public async UniTask Initialize()
        {
            ResolveDependencies();
            SpawnPlayer();

            // PlayerData를 이벤트로 전달하여 EnemyPool 초기화 요청
            EventBus.Publish(this, new PlayerSpawnedEvent(_playerData));
        }

        // EnemyPool 초기화 완료 후 나머지 초기화 진행
        private void OnEnemyPoolInitialized(EnemyPoolInitializedEvent evt)
        {
            InitializeStageManager(evt.EnemyPool);
            DirectingManager.Instance.Initialize();
        }

        // DI에서 종속성 가져오기
        private void ResolveDependencies()
        {
            _battleSettings = DIResolver.Resolve<BattleManagerSettings>();
            _stageSettings = DIResolver.Resolve<StageSettings>();
            _sceneContainer = DIResolver.Resolve<BattleSceneContainer>();
        }

        // 플레이어 생성
        private void SpawnPlayer()
        {
            Vector3 spawnPos = _sceneContainer.playerSpawnPoint.position;
            GameObject playerObj = Instantiate(_battleSettings.playerPrefab, spawnPos, Quaternion.identity);
            _player = playerObj.GetComponent<PlayerUnit>();
            _playerData = _player.CreateDataProvider();

            SetCinemachineTarget();
        }

        // Cinemachine 카메라 타겟 설정
        private void SetCinemachineTarget()
        {
            _sceneContainer.cinemachineCamera.Follow = _player.transform;
        }

        // StageManager 초기화
        private void InitializeStageManager(EnemyPool enemyPool)
        {
            _stageManager = new StageManager(
                _stageSettings,
                enemyPool,
                _sceneContainer.enemySpawnPoints
            );
            _stageManager.SubscribeEvents();

            // StageUI 초기화
            _sceneContainer.stageUI.Initialize(_stageManager);
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

        // 모든 스테이지 클리어 이벤트 핸들러 - 직접 팝업 표시
        private void OnAllStagesCleared(AllStagesClearedEvent evt)
        {
            PopupManager.Instance.Push<GameClearPopup>();
        }

        // 게임 오버 이벤트 핸들러 - 직접 팝업 표시
        private void OnGameOver(GameOverEvent evt)
        {
            PopupManager.Instance.Push<GameOverPopup>();
        }

        // 전투 데이터 정리 (InGameSceneEntry에서 호출)
        public void ResetBattleData()
        {
            // StageManager 정리 (활성 적 먼저 정리)
            if (_stageManager != null)
            {
                _stageManager.ClearAllEnemies();
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

            // DirectingManager 정리
            DirectingManager.Instance.Dispose();
        }

        // 정리
        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.UnsubscribeEvents();
            }
        }
    }
}
