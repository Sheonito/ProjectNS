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
        [Tooltip("공격 상태 목표 지속 시간 (초)")]
        public float attackTargetDuration = 0.4f;
        [Tooltip("공격 데미지")]
        public int attackDamage = 10;
        [Tooltip("공격 판정 타이밍 비율 (0~1, 목표 지속 시간 기준)")]
        [Range(0f, 1f)]
        public float attackHitTimingRatio = 0.5f;
        [Tooltip("공격 범위")]
        public float attackRange = 1.5f;

        [Header("Slash Attack (마우스 아래 공격)")]
        [Tooltip("슬래시 공격 시 살짝 대시 거리")]
        public float slashDashDistance = 1.5f;
        [Tooltip("슬래시 대시 진행 비율 (0~1, 목표 지속 시간 기준)")]
        [Range(0f, 1f)]
        public float slashDashRatio = 0.3f;

        [Header("Jump Attack (마우스 위 공격)")]
        [Tooltip("점프 공격 상태 목표 지속 시간 (초)")]
        public float jumpAttackTargetDuration = 0.5f;
        [Tooltip("점프 공격 시 전방 이동 속도")]
        public float jumpAttackForwardSpeed = 8f;
        [Tooltip("점프 공격 시 점프력 배율")]
        public float jumpAttackJumpMultiplier = 0.8f;

        [Header("Dash Attack")]
        [Tooltip("대시 공격 상태 목표 지속 시간 (초)")]
        public float dashAttackTargetDuration = 0.5f;
        [Tooltip("대시 거리")]
        public float dashDistance = 5f;
        [Tooltip("대시 이동 비율 (0~1, 목표 지속 시간 기준)")]
        [Range(0f, 1f)]
        public float dashMoveRatio = 0.5f;
        [Tooltip("대시 공격 데미지 적용 타이밍 비율 (0~1, 대시 이동 구간 기준)")]
        [Range(0f, 1f)]
        public float dashAttackHitTimingRatio = 0.5f;
        [Tooltip("대시 데미지")]
        public int dashDamage = 15;
        [Tooltip("대시 후 딜레이 (후딜레이)")]
        public float dashRecoveryTime = 0.15f;
        [Tooltip("대시 공격 쿨타임")]
        public float dashCooldown = 1.0f;

        [Header("Backstep")]
        [Tooltip("백스텝 상태 목표 지속 시간 (초)")]
        public float backstepTargetDuration = 0.3f;
        [Tooltip("백스텝 거리")]
        public float backstepDistance = 3f;
        [Tooltip("백스텝 이동 비율 (0~1, 목표 지속 시간 기준)")]
        [Range(0f, 1f)]
        public float backstepMoveRatio = 0.6f;
        [Tooltip("백스텝 후딜레이")]
        public float backstepRecoveryTime = 0.1f;
        [Tooltip("백스텝 쿨타임")]
        public float backstepCooldown = 0.5f;

        [Header("Dive Attack (점프 찍기 공격)")]
        [Tooltip("점프 찍기 공격 상태 목표 지속 시간 (초)")]
        public float diveAttackTargetDuration = 0.4f;
        [Tooltip("점프 찍기 공격 데미지")]
        public int diveAttackDamage = 20;
        [Tooltip("점프 찍기 하강 속도")]
        public float diveAttackSpeed = 25f;
        [Tooltip("점프 찍기 수평 이동 속도")]
        public float diveAttackHorizontalSpeed = 5f;
        [Tooltip("점프 찍기 공격 판정 범위")]
        public float diveAttackRange = 1.2f;
        [Tooltip("점프 찍기 착지 후 경직 시간")]
        public float diveAttackRecoveryTime = 0.2f;

        [Header("Combat")]
        [Tooltip("적 레이어")]
        public LayerMask enemyLayer;

        [Header("Damaged")]
        [Tooltip("피격 상태 목표 지속 시간 (초)")]
        public float damagedTargetDuration = 0.3f;

        [Header("Death")]
        [Tooltip("사망 상태 목표 지속 시간 (초)")]
        public float deathTargetDuration = 1f;
    }
}
