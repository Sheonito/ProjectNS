using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 상태별 설정값 SO
    [CreateAssetMenu(fileName = "EnemyStateSettings", menuName = "ProjectNS/Enemy/State Settings")]
    public class EnemyStateSettings : ScriptableObject
    {
        [Header("Idle")]
        public EnemyIdleSettings idle = new EnemyIdleSettings();

        [Header("Patrol")]
        public EnemyPatrolSettings patrol = new EnemyPatrolSettings();

        [Header("Attack")]
        public EnemyAttackSettings attack = new EnemyAttackSettings();

        [Header("Damaged")]
        public EnemyDamagedSettings damaged = new EnemyDamagedSettings();

        [Header("Death")]
        public EnemyDeathSettings death = new EnemyDeathSettings();
    }

    [System.Serializable]
    public class EnemyIdleSettings
    {
        [Tooltip("대기 상태 유지 시간")]
        public float duration = 2f;
    }

    [System.Serializable]
    public class EnemyPatrolSettings
    {
        [Tooltip("순찰 상태 유지 시간")]
        public float duration = 3f;
    }

    [System.Serializable]
    public class EnemyAttackSettings
    {
        [Tooltip("공격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.7f;
        
        [Tooltip("공격 판정 타이밍 비율 (0~1)")]
        [Range(0f, 1f)]
        public float hitTimingRatio = 0.5f;
        
        [Tooltip("공격 데미지")]
        public int damage = 10;
        
        [Tooltip("공격 쿨타임 (초)")]
        public float cooldown = 1f;
        
        [Tooltip("공격 범위")]
        public float range = 1.5f;
    }

    [System.Serializable]
    public class EnemyDamagedSettings
    {
        [Tooltip("피격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.5f;
    }

    [System.Serializable]
    public class EnemyDeathSettings
    {
        [Tooltip("사망 상태 목표 지속 시간 (초)")]
        public float targetDuration = 1f;
    }
}
