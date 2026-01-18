using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 투사체 설정 SO
    [CreateAssetMenu(fileName = "ProjectileSettings", menuName = "ProjectNS/Enemy/Projectile Settings")]
    public class ProjectileSettings : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("투사체 속도")]
        public float speed = 15f;

        [Tooltip("투사체 생존 시간")]
        public float lifeTime = 3f;

        [Header("Collision")]
        [Tooltip("플레이어 레이어")]
        public LayerMask playerLayer;

        [Tooltip("지형 레이어 (벽/바닥)")]
        public LayerMask groundLayer;
    }
}
