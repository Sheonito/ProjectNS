using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Effect;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Spawner
{
    // 이펙트 스폰 관리 (히트 이펙트 등)
    public class EffectSpawner
    {
        private readonly HitEffect _hitEffectPrefab;
        private readonly int _preLoadCount;

        private HitEffectPool _hitEffectPool;
        private Transform _poolParent;

        public EffectSpawner(HitEffect hitEffectPrefab, int preLoadCount = 20)
        {
            _hitEffectPrefab = hitEffectPrefab;
            _preLoadCount = preLoadCount;
        }

        public void Initialize(Transform parent)
        {
            CreatePoolParent(parent);
            InitializePool();
        }

        public void Dispose()
        {
            _hitEffectPool?.UnsubscribeEvents();
        }

        private void CreatePoolParent(Transform parent)
        {
            GameObject poolObj = new GameObject("EffectSpawnerPool");
            poolObj.transform.SetParent(parent);
            _poolParent = poolObj.transform;
        }

        private void InitializePool()
        {
            if (_hitEffectPrefab != null)
            {
                _hitEffectPool = new HitEffectPool(_hitEffectPrefab, _poolParent, _preLoadCount);
            }
        }

        // 히트 이펙트 스폰
        public void SpawnHitEffect(Vector3 position)
        {
            if (_hitEffectPool == null) return;

            HitEffect effect = _hitEffectPool.Spawn(position);
            if (effect != null)
            {
                effect.transform.position = position;
                effect.gameObject.SetActive(true);
            }
        }

        // 모든 이펙트 정리
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
