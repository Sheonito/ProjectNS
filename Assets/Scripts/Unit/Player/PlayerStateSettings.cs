using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태별 설정값 SO
    [CreateAssetMenu(fileName = "PlayerStateSettings", menuName = "ProjectNS/Player/State Settings")]
    public class PlayerStateSettings : ScriptableObject
    {
        [Header("Character")]
        public CharacterSettings character = new CharacterSettings();

        [Header("Attack (슬래시)")]
        public AttackSettings attack = new AttackSettings();

        [Header("Jump Attack (마우스 위)")]
        public JumpAttackSettings jumpAttack = new JumpAttackSettings();

        [Header("Dash Attack")]
        public DashAttackSettings dashAttack = new DashAttackSettings();

        [Header("Backstep")]
        public BackstepSettings backstep = new BackstepSettings();

        [Header("Dive Attack (점프 찍기)")]
        public DiveAttackSettings diveAttack = new DiveAttackSettings();

        [Header("Combat")]
        public CombatSettings combat = new CombatSettings();

        [Header("Damaged")]
        public DamagedSettings damaged = new DamagedSettings();

        [Header("Death")]
        public DeathSettings death = new DeathSettings();

        [Header("Shield")]
        public ShieldSettings shield = new ShieldSettings();
    }

    [System.Serializable]
    public class CharacterSettings
    {
        [Tooltip("캐릭터 중심점 Y 오프셋 (마우스 위/아래 판정 기준)")]
        public float centerOffset = 0.5f;
    }

    [System.Serializable]
    public class AttackSettings
    {
        [Tooltip("공격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.3f;
        
        [Tooltip("공격 데미지")]
        public int damage = 10;
        
        [Tooltip("공격 판정 타이밍 비율 (0~1)")]
        [Range(0f, 1f)]
        public float hitTimingRatio = 0.8f;
        
        [Tooltip("공격 범위")]
        public float range = 1.5f;
        
        [Tooltip("다수 적 타격 시 간격 (초)")]
        public float hitInterval = 0.075f;
        
        [Tooltip("일반 공격 쿨타임 (초)")]
        public float cooldown = 0.5f;
        
        [Tooltip("슬래시 공격 시 살짝 대시 거리")]
        public float slashDashDistance = 1.5f;
        
        [Tooltip("슬래시 대시 진행 비율 (0~1)")]
        [Range(0f, 1f)]
        public float slashDashRatio = 0.5f;
    }

    [System.Serializable]
    public class JumpAttackSettings
    {
        [Tooltip("점프 공격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.4f;
        
        [Tooltip("점프 공격 시 전방 이동 속도")]
        public float forwardSpeed = 12f;
        
        [Tooltip("점프 공격 시 점프력 배율")]
        public float jumpMultiplier = 0.6f;
        
        [Tooltip("점프 공격 쿨타임 (초)")]
        public float cooldown = 0.6f;
    }

    [System.Serializable]
    public class DashAttackSettings
    {
        [Tooltip("대시 공격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.5f;
        
        [Tooltip("대시 거리")]
        public float distance = 5f;
        
        [Tooltip("대시 이동 비율 (0~1)")]
        [Range(0f, 1f)]
        public float moveRatio = 0.5f;
        
        [Tooltip("대시 공격 데미지 적용 타이밍 비율 (0~1)")]
        [Range(0f, 1f)]
        public float hitTimingRatio = 0.5f;
        
        [Tooltip("대시 데미지")]
        public int damage = 15;
        
        [Tooltip("대시 후 딜레이 (후딜레이)")]
        public float recoveryTime = 0.15f;
        
        [Tooltip("대시 공격 쿨타임")]
        public float cooldown = 1f;
    }

    [System.Serializable]
    public class BackstepSettings
    {
        [Tooltip("백스텝 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.3f;
        
        [Tooltip("백스텝 거리")]
        public float distance = 3f;
        
        [Tooltip("백스텝 이동 비율 (0~1)")]
        [Range(0f, 1f)]
        public float moveRatio = 0.6f;
        
        [Tooltip("백스텝 후딜레이")]
        public float recoveryTime = 0.1f;
        
        [Tooltip("백스텝 쿨타임")]
        public float cooldown = 0.5f;
    }

    [System.Serializable]
    public class DiveAttackSettings
    {
        [Tooltip("점프 찍기 공격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.4f;
        
        [Tooltip("점프 찍기 공격 데미지")]
        public int damage = 20;
        
        [Tooltip("점프 찍기 하강 속도")]
        public float speed = 20f;
        
        [Tooltip("점프 찍기 수평 이동 속도")]
        public float horizontalSpeed = 20f;
        
        [Tooltip("점프 찍기 공격 판정 범위")]
        public float range = 0.6f;
        
        [Tooltip("점프 찍기 착지 후 경직 시간")]
        public float recoveryTime = 0.1f;
        
        [Tooltip("점프 찍기 적 히트 시 튕김 힘 (위쪽)")]
        public float bounceForce = 10f;
        
        [Tooltip("점프 찍기 적 히트 시 튕김 힘 (뒤쪽)")]
        public float bounceBackForce = 11f;
        
        [Tooltip("점프 찍기 공격 쿨타임 (초)")]
        public float cooldown = 0.5f;
    }

    [System.Serializable]
    public class CombatSettings
    {
        [Tooltip("적 레이어")]
        public LayerMask enemyLayer;
    }

    [System.Serializable]
    public class DamagedSettings
    {
        [Tooltip("피격 상태 목표 지속 시간 (초)")]
        public float targetDuration = 0.3f;
    }

    [System.Serializable]
    public class DeathSettings
    {
        [Tooltip("사망 상태 목표 지속 시간 (초)")]
        public float targetDuration = 1f;
    }

    [System.Serializable]
    public class ShieldSettings
    {
        [Tooltip("방어막 파괴 후 무적 시간 (초)")]
        public float invincibleDuration = 0.5f;
    }
}
