using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.Spawner;
using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.Directing
{
    // 연출 관리자 (히트스탑 + 카메라 쉐이크)
    public class DirectingManager : SingletonMonoBehaviour<DirectingManager>
    {
        private DirectingContainer _directingContainer;

        private CinemachineImpulseSource _impulseSource;
        private float _hitStopDuration;
        private float _hitStopTimeScale;
        private float _defaultShakeForce;

        private EffectSpawner _effectSpawner;
        private bool _isHitStopping;

        public override void Initialize()
        {
            base.Initialize();
            _directingContainer = DIResolver.Resolve<DirectingContainer>();
            _impulseSource = _directingContainer.impulseSource;
            _hitStopDuration = _directingContainer.HitStopDuration;
            _hitStopTimeScale = _directingContainer.HitStopTimeScale;
            _defaultShakeForce = _directingContainer.CameraShakeForce;
        }

        // EffectSpawner 외부 설정 (InGameSceneEntry에서 호출)
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
            _effectSpawner = null;
        }

        // 카메라 쉐이크 재생
        public void PlayCameraShake(float force = -1)
        {
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
            _effectSpawner.SpawnHitEffect(position);
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
    }
}
