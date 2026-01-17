using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 이동 설정 SO
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "ProjectNS/Player/Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Movement Speed")]
        [Tooltip("지상 이동 속도")]
        public float runSpeed = 12f;
        [Tooltip("공중 이동 속도")]
        public float airSpeed = 10f;

        [Header("Acceleration")]
        [Tooltip("지상 가속도")]
        public float groundAcceleration = 80f;
        [Tooltip("지상 감속도")]
        public float groundDeceleration = 100f;
        [Tooltip("공중 가속도")]
        public float airAcceleration = 40f;
        [Tooltip("공중 감속도")]
        public float airDeceleration = 30f;
        [Tooltip("방향 전환 시 가속 배율")]
        public float turnSpeedMultiplier = 2f;

        [Header("Jump")]
        [Tooltip("점프 힘")]
        public float jumpForce = 18f;
        [Tooltip("중력")]
        public float gravity = -50f;
        [Tooltip("최대 낙하 속도")]
        public float maxFallSpeed = -30f;
        [Tooltip("코요테 타임 (점프 유예 시간)")]
        public float coyoteTime = 0.08f;
        [Tooltip("점프 버튼 떼면 낮게 점프하는 배율")]
        public float jumpCutMultiplier = 0.4f;

        [Header("Ground Check")]
        [Tooltip("바닥 체크 거리")]
        public float groundCheckDistance = 0.15f;
        [Tooltip("바닥 체크 오프셋")]
        public float groundCheckOffset = 0.5f;
        [Tooltip("바닥 레이어")]
        public LayerMask groundLayer = 1;

        [Header("Wall Check")]
        [Tooltip("벽 체크 거리")]
        public float wallCheckDistance = 0.3f;
        [Tooltip("벽 체크 높이 오프셋")]
        public float wallCheckHeight = 0.3f;
    }
}
