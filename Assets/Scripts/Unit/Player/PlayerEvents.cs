using System;
using Percent111.ProjectNS.Event;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태 변경 요청 이벤트 (State → StateMachine)
    public class PlayerChangeStateRequestEvent : IEvent
    {
        public PlayerStateType RequestedState { get; private set; }

        public PlayerChangeStateRequestEvent(PlayerStateType requestedState)
        {
            RequestedState = requestedState;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 상태 변경 완료 이벤트 (StateMachine → Player)
    public class PlayerStateChangedEvent : IEvent
    {
        public PlayerStateType PreviousState { get; private set; }
        public PlayerStateType CurrentState { get; private set; }

        public PlayerStateChangedEvent(PlayerStateType previousState, PlayerStateType currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateMachine);
        }
    }

    // 플레이어 착지 이벤트 (Movement → State/Player)
    public class PlayerLandedEvent : IEvent
    {
        public float FallVelocity { get; private set; }

        public PlayerLandedEvent(float fallVelocity)
        {
            FallVelocity = fallVelocity;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerMovement);
        }
    }

    // 플레이어 점프 이벤트 (State → Player)
    public class PlayerJumpEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 데미지 이벤트 (Player → State)
    public class PlayerDamageEvent : IEvent
    {
        public int Damage { get; private set; }
        public int CurrentHp { get; private set; }

        public PlayerDamageEvent(int damage, int currentHp)
        {
            Damage = damage;
            CurrentHp = currentHp;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerUnit);
        }
    }

    // 플레이어 사망 이벤트 (Player → State)
    public class PlayerDeathEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerUnit);
        }
    }

    // 플레이어 공격 이벤트 (State → Player, 사운드/이펙트용)
    public class PlayerAttackEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 대시공격 이벤트 (State → Player, 사운드/이펙트용)
    public class PlayerDashAttackEvent : IEvent
    {
        public int Direction { get; private set; }

        public PlayerDashAttackEvent(int direction)
        {
            Direction = direction;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }
}
