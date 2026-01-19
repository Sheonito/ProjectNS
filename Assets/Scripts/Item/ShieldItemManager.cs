using System.Collections.Generic;
using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Item
{
    // 방어막 아이템 관리 (드롭, 풀링)
    public class ShieldItemManager : SingletonMonoBehaviour<ShieldItemManager>
    {
        [Header("Shield Item")]
        [SerializeField] private ShieldItem _shieldItemPrefab;
        [SerializeField] private int _preLoadCount = 10;

        [Header("Drop Settings")]
        [Tooltip("방어막 아이템 드롭 확률 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _dropChance = 0.2f;

        private Queue<ShieldItem> _pool;
        private Transform _poolParent;

        public override void Initialize()
        {
            base.Initialize();
            CreatePoolParent();
            InitializePool();
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void CreatePoolParent()
        {
            GameObject poolObj = new GameObject("ShieldItemPool");
            poolObj.transform.SetParent(transform);
            _poolParent = poolObj.transform;
        }

        private void InitializePool()
        {
            _pool = new Queue<ShieldItem>();

            if (_shieldItemPrefab != null)
            {
                for (int i = 0; i < _preLoadCount; i++)
                {
                    ShieldItem item = Instantiate(_shieldItemPrefab, _poolParent);
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
                item = Instantiate(_shieldItemPrefab, _poolParent);
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
    }
}
