using System;
using System.Collections.Generic;
using Percent111.ProjectNS.Enemy;
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

        public int CurrentStage => _currentStage;
        public int MaxStage => _settings.maxStage;
        public bool IsStageCleared => _remainingEnemies <= 0;

        public event Action<int> OnStageStarted;
        public event Action<int> OnStageCleared;
        public event Action OnAllStagesCleared;

        // 생성자
        public StageManager(StageSettings settings, EnemyPool enemyPool, List<Transform> spawnPoints)
        {
            _settings = settings;
            _enemyPool = enemyPool;
            _spawnPoints = spawnPoints;
            _activeEnemies = new List<EnemyUnit>();
            _currentStage = 0;
        }

        // 스테이지 시작
        public void StartStage(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > _settings.maxStage)
            {
                Debug.LogError($"StageManager: Invalid stage number {stageNumber}");
                return;
            }

            _currentStage = stageNumber;
            int enemyCount = GetEnemyCountForStage(stageNumber);

            Debug.Log($"Stage {stageNumber} Start! Enemy Count: {enemyCount}");

            SpawnEnemies(enemyCount);
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

        // 적 스폰
        private void SpawnEnemies(int count)
        {
            _remainingEnemies = count;
            _activeEnemies.Clear();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetSpawnPosition(i);
                EnemyType type = GetEnemyTypeForStage(i);

                EnemyUnit enemy = _enemyPool.Spawn(type, spawnPos);
                if (enemy != null)
                {
                    _activeEnemies.Add(enemy);
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
            Debug.Log($"Stage {_currentStage} Cleared!");
            OnStageCleared?.Invoke(_currentStage);

            if (_currentStage >= _settings.maxStage)
            {
                Debug.Log("Congratulations! All stages cleared!");
                OnAllStagesCleared?.Invoke();
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
