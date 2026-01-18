using System.Collections.Generic;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 타입별 적 풀 관리자
    public class EnemyPool
    {
        private readonly Dictionary<EnemyType, SingleEnemyPool> _pools;
        private readonly Transform _poolParent;
        private readonly PlayerDataProvider _playerData;

        public EnemyPool(Transform poolParent, PlayerDataProvider playerData)
        {
            _pools = new Dictionary<EnemyType, SingleEnemyPool>();
            _poolParent = poolParent;
            _playerData = playerData;
        }

        // 프리팹 등록
        public void RegisterPrefab(EnemyType type, GameObject prefab)
        {
            if (!_pools.ContainsKey(type))
            {
                _pools[type] = new SingleEnemyPool(prefab, _poolParent, _playerData);
            }
        }

        // 풀 미리 생성
        public void PreLoad(EnemyType type, int count)
        {
            if (_pools.TryGetValue(type, out SingleEnemyPool pool))
            {
                pool.PreLoad(count);
            }
        }

        // 적 스폰 (풀에서 가져오기)
        public EnemyUnit Spawn(EnemyType type, Vector3 position)
        {
            if (!_pools.TryGetValue(type, out SingleEnemyPool pool))
            {
                Debug.LogError($"EnemyPool: Prefab not registered for type {type}");
                return null;
            }

            return pool.Spawn(position);
        }

        // 적 디스폰 (풀에 반환)
        public void Despawn(EnemyUnit enemy, EnemyType type)
        {
            if (_pools.TryGetValue(type, out SingleEnemyPool pool))
            {
                pool.Despawn(enemy);
            }
        }

        // 모든 풀 정리
        public void ClearAll()
        {
            foreach (SingleEnemyPool pool in _pools.Values)
            {
                pool.ClearAll();
            }
            _pools.Clear();
        }
    }
}
