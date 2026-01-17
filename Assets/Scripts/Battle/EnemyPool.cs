using System.Collections.Generic;
using Percent111.ProjectNS.Enemy;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 적 오브젝트 풀 (타입별 관리)
    public class EnemyPool
    {
        private readonly Dictionary<EnemyType, Queue<EnemyUnit>> _pools;
        private readonly Dictionary<EnemyType, GameObject> _prefabs;
        private readonly Transform _poolParent;
        private readonly Transform _playerTransform;

        // 생성자
        public EnemyPool(Transform poolParent, Transform playerTransform)
        {
            _pools = new Dictionary<EnemyType, Queue<EnemyUnit>>();
            _prefabs = new Dictionary<EnemyType, GameObject>();
            _poolParent = poolParent;
            _playerTransform = playerTransform;
        }

        // 프리팹 등록
        public void RegisterPrefab(EnemyType type, GameObject prefab)
        {
            _prefabs[type] = prefab;

            if (!_pools.ContainsKey(type))
            {
                _pools[type] = new Queue<EnemyUnit>();
            }
        }

        // 풀 미리 생성
        public void PreLoad(EnemyType type, int count)
        {
            if (!_prefabs.ContainsKey(type)) return;

            for (int i = 0; i < count; i++)
            {
                EnemyUnit enemy = CreateEnemy(type);
                enemy.gameObject.SetActive(false);
                _pools[type].Enqueue(enemy);
            }
        }

        // 적 스폰 (풀에서 가져오기)
        public EnemyUnit Spawn(EnemyType type, Vector3 position)
        {
            if (!_prefabs.ContainsKey(type))
            {
                Debug.LogError($"EnemyPool: Prefab not registered for type {type}");
                return null;
            }

            EnemyUnit enemy;

            if (_pools[type].Count > 0)
            {
                enemy = _pools[type].Dequeue();
            }
            else
            {
                enemy = CreateEnemy(type);
            }

            enemy.transform.position = position;
            enemy.gameObject.SetActive(true);
            enemy.SetPlayerTransform(_playerTransform);

            return enemy;
        }

        // 적 디스폰 (풀에 반환)
        public void Despawn(EnemyUnit enemy, EnemyType type)
        {
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(_poolParent);

            if (!_pools.ContainsKey(type))
            {
                _pools[type] = new Queue<EnemyUnit>();
            }

            _pools[type].Enqueue(enemy);
        }

        // 적 생성 (내부용)
        private EnemyUnit CreateEnemy(EnemyType type)
        {
            GameObject prefab = _prefabs[type];
            GameObject obj = Object.Instantiate(prefab, _poolParent);
            EnemyUnit enemy = obj.GetComponent<EnemyUnit>();
            return enemy;
        }

        // 모든 풀 정리
        public void ClearAll()
        {
            foreach (Queue<EnemyUnit> pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    EnemyUnit enemy = pool.Dequeue();
                    if (enemy != null)
                    {
                        Object.Destroy(enemy.gameObject);
                    }
                }
            }
            _pools.Clear();
        }
    }
}
