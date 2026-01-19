using System;
using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Spawner;
using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.Directing
{
    // 연출 관리자 (히트스탑 + 카메라 쉐이크)
    public class DirectingManager
    {
        private readonly CinemachineImpulseSource _impulseSource;
        private readonly float _hitStopDuration;
        private readonly float _hitStopTimeScale;
        private readonly float _defaultShakeForce;

        private EffectSpawner _effectSpawner;
        private bool _isHitStopping;

        public DirectingManager(
            CinemachineImpulseSource impulseSource,
            float hitStopDuration = 0.05f,
            float hitStopTimeScale = 0f,
            float defaultShakeForce = 0.5f)
        {
            _impulseSource = impulseSource;
            _hitStopDuration = hitStopDuration;
            _hitStopTimeScale = hitStopTimeScale;
            _defaultShakeForce = defaultShakeForce;
        }

        public void SetEffectSpawner(EffectSpawner effectSpawner)
        {
            _effectSpawner = effectSpawner;
        }

        public void Dispose()
        {
            // 파괴 시 timeScale 복원
            if (_isHitStopping)
            {
                Time.timeScale = 1f;
            }
        }

        // 카메라 쉐이크 재생
        public void PlayCameraShake(float force = -1)
        {
            if (_impulseSource == null) return;
            
            float actualForce = force < 0 ? _defaultShakeForce : force;
            _impulseSource.GenerateImpulse(actualForce);
        }

        // 히트스탑 재생
        public void PlayHitStop()
        {
            if (_isHitStopping) return;
            HitStopAsync().Forget();
        }

        // 히트 연출 (카메라 쉐이크 + 히트스탑)
        public void PlayHitFeedback()
        {
            PlayCameraShake();
            PlayHitStop();
        }

        // 타격 연출 (히트스탑 + 카메라 쉐이크 + 이펙트)
        public void PlayHitEffect(Vector3 position)
        {
            PlayHitFeedback();
            _effectSpawner?.SpawnHitEffect(position);
        }

        private async UniTaskVoid HitStopAsync()
        {
            _isHitStopping = true;
            float originalTimeScale = Time.timeScale;
            Time.timeScale = _hitStopTimeScale;

            // realtime으로 대기 (timeScale 영향 받지 않음)
            await UniTask.Delay((int)(_hitStopDuration * 1000), DelayType.Realtime);

            Time.timeScale = originalTimeScale;
            _isHitStopping = false;
        }
    }
}
