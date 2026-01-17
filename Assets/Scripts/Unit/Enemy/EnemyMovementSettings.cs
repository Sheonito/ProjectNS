using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 이동 및 탐지 설정 (ScriptableObject)
    [CreateAssetMenu(fileName = "EnemyMovementSettings", menuName = "ProjectNS/Enemy/MovementSettings")]
    public class EnemyMovementSettings : ScriptableObject
    {
        [Header("이동")]
        public float patrolSpeed = 2f;
        public float chaseSpeed = 5f;
        public float acceleration = 20f;
        public float deceleration = 30f;

        [Header("플레이어 탐지 (카타나 제로 스타일)")]
        public float detectionRange = 8f;           // 탐지 거리
        public bool useFieldOfView = false;         // 시야각 사용 여부 (false = 360도 탐지)
        public float detectionAngle = 90f;          // 시야각 (useFieldOfView가 true일 때만 적용)
        public float loseTargetTime = 2f;           // 시야 밖으로 나간 후 추적 유지 시간
        public LayerMask playerLayer;               // 플레이어 레이어
        public LayerMask obstacleLayer;             // 장애물 레이어 (벽 등)

        [Header("공격")]
        public float attackRange = 1.5f;            // 공격 범위
        public float attackCooldown = 1f;           // 공격 쿨타임

        [Header("순찰")]
        public float patrolWaitTime = 2f;           // 순찰 지점 대기 시간
        public float patrolDistance = 3f;           // 순찰 거리

        [Header("지면/벽 감지")]
        public float groundCheckDistance = 0.1f;
        public float groundCheckOffset = 0.1f;
        public float wallCheckDistance = 0.3f;
        public float wallCheckHeight = 0.5f;
        public LayerMask groundLayer;

        [Header("중력")]
        public float gravity = -40f;
        public float maxFallSpeed = -20f;
    }
}
