using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 스테이지 관리자 (일반 C# 클래스, BattleManager에서 생성)
    public class StageManager
    {
        private readonly StageSettings _settings;
        private readonly EnemyPool _enemyPool;
        private readonly List<Transform> _spawnPoints;

        private int _currentStage;
        private int _remainingEnemies;
        private List<EnemyUnit> _activeEnemies;
        private int _separationFrame;

        // 타이머
        private float _stageTimer;
        private bool _isTimerRunning;

        // 스폰 취소 토큰
        private CancellationTokenSource _spawnCts;

        public int CurrentStage => _currentStage;
        public int MaxStage => _settings.maxStage;
        public bool IsStageCleared => _remainingEnemies <= 0;
        public float RemainingTime => Mathf.Max(0, _stageTimer);
        public float StageDuration => _settings.stageDuration;

        public event Action<int> OnStageStarted;
        public event Action<int> OnStageCleared;
        public event Action OnAllStagesCleared;
        public event Action<float> OnTimerUpdated;

        // 생성자
        public StageManager(StageSettings settings, EnemyPool enemyPool, List<Transform> spawnPoints)
        {
            _settings = settings;
            _enemyPool = enemyPool;
            _spawnPoints = spawnPoints;
            _activeEnemies = new List<EnemyUnit>();
            _currentStage = 0;
        }

        // 이벤트 구독 (BattleManager에서 호출)
        public void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyReturnToPoolEvent>(OnEnemyReturnToPool);
        }

        // 이벤트 구독 해제 (BattleManager에서 호출)
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyReturnToPoolEvent>(OnEnemyReturnToPool);

            // 스폰 취소 및 정리
            _spawnCts?.Cancel();
            _spawnCts?.Dispose();
            _spawnCts = null;
        }

        // 적 Pool 반환 이벤트 핸들러
        private void OnEnemyReturnToPool(EnemyReturnToPoolEvent evt)
        {
            EnemyUnit enemy = evt.Enemy;

            _remainingEnemies--;
            _activeEnemies.Remove(enemy);

            // 상태 초기화 후 Pool 반환
            enemy.ResetForPool();
            _enemyPool.Despawn(enemy, EnemyType.Melee); // TODO: EnemyUnit에서 Type 가져오기

            Debug.Log($"Enemy returned to pool! Remaining: {_remainingEnemies}");

            if (IsStageCleared)
            {
                HandleStageClear();
            }
        }

        // 스테이지 시작
        public void StartStage(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > _settings.maxStage)
            {
                Debug.LogError($"StageManager: Invalid stage number {stageNumber}");
                return;
            }

            // 이전 스폰 취소
            _spawnCts?.Cancel();
            _spawnCts?.Dispose();
            _spawnCts = new CancellationTokenSource();

            _currentStage = stageNumber;
            int enemyCount = GetEnemyCountForStage(stageNumber);

            // 타이머 시작
            _stageTimer = _settings.stageDuration;
            _isTimerRunning = true;

            Debug.Log($"Stage {stageNumber} Start! Enemy Count: {enemyCount}, Duration: {_settings.stageDuration}s");

            SpawnEnemiesAsync(enemyCount, _spawnCts.Token).Forget();
            OnStageStarted?.Invoke(stageNumber);
        }

        // 다음 스테이지 시작
        public void StartNextStage()
        {
            if (_currentStage >= _settings.maxStage)
            {
                Debug.Log("All stages cleared!");
                return;
            }

            StartStage(_currentStage + 1);
        }

        // 스테이지별 적 수 계산
        private int GetEnemyCountForStage(int stageNumber)
        {
            return _settings.baseEnemyCount + (_settings.enemyIncreasePerStage * (stageNumber - 1));
        }

        // 적 스폰 (interval 적용)
        private async UniTaskVoid SpawnEnemiesAsync(int count, CancellationToken ct)
        {
            _remainingEnemies = count;
            _activeEnemies.Clear();

            for (int i = 0; i < count; i++)
            {
                if (ct.IsCancellationRequested) return;

                Vector3 spawnPos = GetSpawnPosition(i);
                EnemyType type = GetEnemyTypeForStage(i);

                EnemyUnit enemy = _enemyPool.Spawn(type, spawnPos);
                if (enemy != null)
                {
                    _activeEnemies.Add(enemy);
                }

                // 마지막 적이 아니면 interval 대기
                if (i < count - 1 && _settings.spawnInterval > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_settings.spawnInterval), cancellationToken: ct);
                }
            }
        }

        // 스폰 위치 반환
        private Vector3 GetSpawnPosition(int index)
        {
            if (_spawnPoints == null || _spawnPoints.Count == 0)
            {
                float randomX = UnityEngine.Random.Range(-5f, 5f);
                return new Vector3(randomX, 0, 0);
            }

            int spawnIndex = index % _spawnPoints.Count;
            return _spawnPoints[spawnIndex].position;
        }

        // 스테이지에 따른 적 타입 결정
        private EnemyType GetEnemyTypeForStage(int enemyIndex)
        {
            float rangedChance = (_currentStage - 1) * _settings.rangedChanceIncreasePerStage;

            if (UnityEngine.Random.value < rangedChance)
            {
                return EnemyType.Ranged;
            }

            return EnemyType.Melee;
        }

        // 적 사망 처리 (EnemyUnit에서 호출)
        public void OnEnemyDeath(EnemyUnit enemy, EnemyType type)
        {
            _remainingEnemies--;
            _activeEnemies.Remove(enemy);

            _enemyPool.Despawn(enemy, type);

            Debug.Log($"Enemy killed! Remaining: {_remainingEnemies}");

            if (IsStageCleared)
            {
                HandleStageClear();
            }
        }

        // 스테이지 클리어 처리
        private void HandleStageClear()
        {
            // 타이머 정지
            _isTimerRunning = false;

            // 스폰 취소
            _spawnCts?.Cancel();

            Debug.Log($"Stage {_currentStage} Cleared!");
            OnStageCleared?.Invoke(_currentStage);

            if (_currentStage >= _settings.maxStage)
            {
                Debug.Log("Congratulations! All stages cleared!");
                OnAllStagesCleared?.Invoke();
            }
            else
            {
                // 다음 스테이지로 자동 이동
                StartNextStage();
            }
        }

        // 모든 활성 적 제거
        public void ClearAllEnemies()
        {
            foreach (EnemyUnit enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    _enemyPool.Despawn(enemy, EnemyType.Melee);
                }
            }

            _activeEnemies.Clear();
            _remainingEnemies = 0;
        }

        // 타이머 업데이트 (BattleManager에서 매 프레임 호출)
        public void UpdateTimer()
        {
            if (!_isTimerRunning) return;

            _stageTimer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(_stageTimer);

            // 시간이 다 되면 자동으로 다음 스테이지
            if (_stageTimer <= 0)
            {
                _isTimerRunning = false;
                HandleStageTimeUp();
            }
        }

        // 스테이지 시간 종료 처리
        private void HandleStageTimeUp()
        {
            Debug.Log($"Stage {_currentStage} Time Up!");

            // 이전 스테이지 몬스터는 유지 (제거하지 않음)
            // 스테이지 클리어 처리 (내부에서 다음 스테이지 자동 이동)
            HandleStageClear();
        }

        // 적끼리 밀어내기 업데이트 (BattleManager에서 매 프레임 호출)
        public void UpdateEnemySeparation()
        {
            _separationFrame++;
            if (_separationFrame % _settings.separationUpdateInterval != 0) return;

            float radius = _settings.separationRadius;
            float force = _settings.separationForce;
            int interval = _settings.separationUpdateInterval;

            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                if (_activeEnemies[i] == null) continue;

                Vector2 separation = Vector2.zero;
                Vector2 posA = _activeEnemies[i].transform.position;

                for (int j = 0; j < _activeEnemies.Count; j++)
                {
                    if (i == j || _activeEnemies[j] == null) continue;

                    Vector2 posB = _activeEnemies[j].transform.position;
                    Vector2 diff = posA - posB;
                    float dist = diff.magnitude;

                    if (dist < radius)
                    {
                        if (dist < 0.01f)
                        {
                            // 완전히 겹친 경우 랜덤 방향
                            diff = new Vector2(UnityEngine.Random.Range(-1f, 1f), 0);
                            dist = 0.01f;
                        }

                        float strength = 1f - (dist / radius);
                        separation += diff.normalized * strength;
                    }
                }

                // 프레임 스킵 보정 적용하여 Enemy에 전달
                _activeEnemies[i].UpdateSeparationForce(separation * force * interval);
            }
        }
    }
}
