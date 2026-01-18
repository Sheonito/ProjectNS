using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 상태별 설정값 SO
    [CreateAssetMenu(fileName = "EnemyStateSettings", menuName = "ProjectNS/Enemy/State Settings")]
    public class EnemyStateSettings : ScriptableObject
    {
        [Header("Idle State")]
        [Tooltip("대기 상태 유지 시간")]
        public float idleDuration = 2f;

        [Header("Patrol State")]
        [Tooltip("순찰 상태 유지 시간")]
        public float patrolDuration = 3f;

        [Header("Attack State")]
        [Tooltip("공격 상태 목표 지속 시간 (초)")]
        public float attackTargetDuration = 0.5f;
        [Tooltip("공격 판정 타이밍 비율 (0~1, 목표 지속 시간 기준)")]
        [Range(0f, 1f)]
        public float attackHitTimingRatio = 0.5f;
        [Tooltip("공격 데미지")]
        public int attackDamage = 10;

        [Header("Damaged State")]
        [Tooltip("피격 상태 목표 지속 시간 (초)")]
        public float damagedTargetDuration = 0.3f;

        [Header("Death State")]
        [Tooltip("사망 상태 목표 지속 시간 (초)")]
        public float deathTargetDuration = 1f;
    }
}
