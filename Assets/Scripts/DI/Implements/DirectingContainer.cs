using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.DI
{
    // 연출 설정 및 종속
    public class DirectingContainer : MonoBehaviour
    {
        public CinemachineImpulseSource impulseSource;
        public float HitStopDuration => hitStopDuration;
        public float HitStopTimeScale => hitStopTimeScale;
        public float CameraShakeForce => cameraShakeForce;
        
        [SerializeField] private float hitStopDuration = 0.075f;
        [SerializeField] private float hitStopTimeScale = 0f;
        [SerializeField] private float cameraShakeForce = 0.1f;
    }
}
