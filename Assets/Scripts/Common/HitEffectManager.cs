using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Effect;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    // 히트 이펙트 총괄 관리 (카메라 쉐이크 + 이펙트 스폰 + 히트스탑)
    public class HitEffectManager : SingletonMonoBehaviour<HitEffectManager>
    {
        [Header("Hit Effect")]
        [SerializeField] private HitEffect _hitEffectPrefab;
        [SerializeField] private int _preLoadCount = 20;

        [Header("Hit Stop")]
        [Tooltip("히트스탑 지속 시간 (초, realtime)")]
        [SerializeField] private float _hitStopDuration = 0.05f;
        [Tooltip("히트스탑 시 timeScale (0 = 완전 정지)")]
        [SerializeField] private float _hitStopTimeScale = 0f;

        private HitEffectPool _hitEffectPool;
        private Transform _poolParent;
        private bool _isHitStopping;

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

        // 히트스탑 재생 (타격 시 시간 멈춤)
        public void PlayHitStop()
        {
            if (_isHitStopping) return;
            HitStopAsync().Forget();
        }

        // 히트스탑 + 카메라 쉐이크 동시 재생
        public void PlayHitFeedback(Vector3 position)
        {
            PlayHitStop();
            PlayCameraShake();
            SpawnHitEffect(position);
        }

        private async UniTaskVoid HitStopAsync()
        {
            _isHitStopping = true;
            float originalTimeScale = Time.timeScale;
            Time.timeScale = _hitStopTimeScale;

            // realtime으로 대기 (timeScale 영향 받지 않음)
            await UniTask.WaitForSeconds(_hitStopDuration, ignoreTimeScale: true);

            Time.timeScale = originalTimeScale;
            _isHitStopping = false;
        }

        private void OnDestroy()
        {
            // 파괴 시 timeScale 복원
            if (_isHitStopping)
            {
                Time.timeScale = 1f;
            }
            _hitEffectPool?.UnsubscribeEvents();
        }
    }
}
