using System;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    // 카메라 쉐이크 및 히트스탑 관리
    public class CameraShakeManager : SingletonMonoBehaviour<CameraShakeManager>
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Hit Stop")]
        [SerializeField] private float _hitStopDuration = 0.05f;

        [Header("Shake")]
        [SerializeField] private float _shakeForce = 0.5f;

        // 타격 연출 (쉐이크 + 히트스탑)
        public void PlayHitEffect(float force = -1)
        {
            float actualForce = force < 0 ? _shakeForce : force;

            // 카메라 쉐이크
            if (_impulseSource != null)
            {
                _impulseSource.GenerateImpulse(actualForce);
            }

            // 히트스탑
            HitStopAsync().Forget();
        }

        // 히트스탑 (프레임 정지)
        private async UniTaskVoid HitStopAsync()
        {
            Time.timeScale = 0f;
            await UniTask.Delay(
                TimeSpan.FromSeconds(_hitStopDuration),
                DelayType.UnscaledDeltaTime
            );
            Time.timeScale = 1f;
        }
    }
}
