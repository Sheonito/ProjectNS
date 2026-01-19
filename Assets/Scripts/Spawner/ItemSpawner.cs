using System.Collections.Generic;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Item;
using UnityEngine;

namespace Percent111.ProjectNS.Spawner
{
    // 아이템 스폰 관리 (드롭, 풀링)
    public class ItemSpawner
    {
        private readonly ShieldItem _shieldItemPrefab;
        private readonly float _dropChance;
        private readonly int _preLoadCount;
        
        private Queue<ShieldItem> _pool;
        private Transform _poolParent;

        public ItemSpawner(ShieldItem shieldItemPrefab, float dropChance, int preLoadCount = 10)
        {
            _shieldItemPrefab = shieldItemPrefab;
            _dropChance = dropChance;
            _preLoadCount = preLoadCount;
        }

        public void Initialize(Transform parent)
        {
            CreatePoolParent(parent);
            InitializePool();
            SubscribeEvents();
        }

        public void Dispose()
        {
            UnsubscribeEvents();
        }

        private void CreatePoolParent(Transform parent)
        {
            GameObject poolObj = new GameObject("ItemSpawnerPool");
            poolObj.transform.SetParent(parent);
            _poolParent = poolObj.transform;
        }

        private void InitializePool()
        {
            _pool = new Queue<ShieldItem>();

            if (_shieldItemPrefab != null)
            {
                for (int i = 0; i < _preLoadCount; i++)
                {
                    ShieldItem item = Object.Instantiate(_shieldItemPrefab, _poolParent);
                    item.gameObject.SetActive(false);
                    _pool.Enqueue(item);
                }
            }
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        // 적 사망 시 랜덤 드롭
        private void OnEnemyDeathComplete(EnemyDeathCompleteEvent evt)
        {
            if (evt.Owner == null) return;

            // 드롭 확률 체크
            if (Random.value <= _dropChance)
            {
                SpawnShieldItem(evt.Owner.GetPosition());
            }
        }

        // 방어막 아이템 스폰
        public void SpawnShieldItem(Vector3 position)
        {
            if (_shieldItemPrefab == null) return;

            ShieldItem item;
            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
            }
            else
            {
                item = Object.Instantiate(_shieldItemPrefab, _poolParent);
            }

            item.transform.position = position;
            item.ResetForPool();
            item.gameObject.SetActive(true);
        }

        // 아이템 반환 (ShieldItem에서 호출)
        public void ReturnToPool(ShieldItem item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            item.transform.SetParent(_poolParent);
            _pool.Enqueue(item);
        }

        // 모든 아이템 정리
        public void ClearAll()
        {
            if (_poolParent == null) return;
            
            foreach (Transform child in _poolParent)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
