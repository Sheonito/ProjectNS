using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    // 히트 이펙트 총괄 관리 (카메라 쉐이크 + 이펙트 스폰)
    public class HitEffectManager : SingletonMonoBehaviour<HitEffectManager>
    {
        [Header("Hit Effect")]
        [SerializeField] private HitEffect _hitEffectPrefab;
        [SerializeField] private int _preLoadCount = 20;

        private HitEffectPool _hitEffectPool;
        private Transform _poolParent;

        public override void Initialize()
        {
            base.Initialize();
            CreatePoolParent();
            InitializePool();
        }

        private void CreatePoolParent()
        {
            GameObject poolObj = new GameObject("HitEffectPool");
            poolObj.transform.SetParent(transform);
            _poolParent = poolObj.transform;
        }

        private void InitializePool()
        {
            if (_hitEffectPrefab != null)
            {
                _hitEffectPool = new HitEffectPool(_hitEffectPrefab, _poolParent, _preLoadCount);
            }
        }

        // 타격 연출 재생 (위치 지정)
        public void PlayHitEffect(Vector3 position)
        {
            // 카메라 쉐이크 + 히트스탑
            CameraShakeManager.Instance?.PlayHitEffect();

            // 히트 이펙트 스폰
            SpawnHitEffect(position);
        }

        // 히트 이펙트만 스폰
        public void SpawnHitEffect(Vector3 position)
        {
            _hitEffectPool?.Spawn(position);
        }

        // 카메라 쉐이크만 재생
        public void PlayCameraShake(float force = -1)
        {
            CameraShakeManager.Instance?.PlayHitEffect(force);
        }

        private void OnDestroy()
        {
            _hitEffectPool?.UnsubscribeEvents();
        }
    }
}
