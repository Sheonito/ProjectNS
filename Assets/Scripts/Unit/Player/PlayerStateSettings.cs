using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태별 설정값 SO
    [CreateAssetMenu(fileName = "PlayerStateSettings", menuName = "ProjectNS/Player/State Settings")]
    public class PlayerStateSettings : ScriptableObject
    {
        [Header("Character")]
        [Tooltip("캐릭터 중심점 Y 오프셋 (마우스 위/아래 판정 기준)")]
        public float characterCenterOffset = 0.5f;

        [Header("Attack")]
        [Tooltip("공격 지속 시간")]
        public float attackDuration = 0.4f;
        [Tooltip("공격 데미지")]
        public int attackDamage = 10;
        [Tooltip("공격 판정 타이밍 (시작 후 경과 시간)")]
        public float attackHitTiming = 0.2f;
        [Tooltip("공격 범위")]
        public float attackRange = 1.5f;

        [Header("Slash Attack (마우스 아래 공격)")]
        [Tooltip("슬래시 공격 시 살짝 대시 거리")]
        public float slashDashDistance = 1.5f;
        [Tooltip("슬래시 대시 지속 시간")]
        public float slashDashDuration = 0.15f;

        [Header("Jump Attack (마우스 위 공격)")]
        [Tooltip("점프 공격 시 전방 이동 속도")]
        public float jumpAttackForwardSpeed = 8f;
        [Tooltip("점프 공격 시 점프력 배율")]
        public float jumpAttackJumpMultiplier = 0.8f;

        [Header("Dash Attack")]
        [Tooltip("대시 거리")]
        public float dashDistance = 5f;
        [Tooltip("대시 지속 시간")]
        public float dashDuration = 0.3f;
        [Tooltip("대시 데미지")]
        public int dashDamage = 15;
        [Tooltip("대시 후 딜레이 (후딜레이)")]
        public float dashRecoveryTime = 0.15f;
        [Tooltip("대시 공격 쿨타임")]
        public float dashCooldown = 1.0f;

        [Header("Combat")]
        [Tooltip("적 레이어")]
        public LayerMask enemyLayer;

        [Header("Damaged")]
        [Tooltip("피격 경직 시간")]
        public float damagedDuration = 0.3f;

        [Header("Death")]
        [Tooltip("사망 애니메이션 지속 시간")]
        public float deathDuration = 1f;
    }
}
