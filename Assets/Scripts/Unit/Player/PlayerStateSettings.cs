using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태별 설정값 SO
    [CreateAssetMenu(fileName = "PlayerStateSettings", menuName = "ProjectNS/Player/State Settings")]
    public class PlayerStateSettings : ScriptableObject
    {
        [Header("Attack")]
        [Tooltip("공격 지속 시간")]
        public float attackDuration = 0.4f;
        [Tooltip("공격 데미지")]
        public int attackDamage = 10;
        [Tooltip("공격 판정 타이밍 (시작 후 경과 시간)")]
        public float attackHitTiming = 0.2f;
        [Tooltip("공격 범위")]
        public float attackRange = 1.5f;

        [Header("Dash Attack")]
        [Tooltip("대시 거리")]
        public float dashDistance = 5f;
        [Tooltip("대시 지속 시간")]
        public float dashDuration = 0.3f;
        [Tooltip("대시 데미지")]
        public int dashDamage = 15;

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
