using System;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Item;

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

    // 플레이어 점프 이벤트 (State → Player)
    public class PlayerJumpEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
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

    // 플레이어 무적 상태 변경 이벤트 (State → Player)
    public class PlayerInvincibleEvent : IEvent
    {
        public bool IsInvincible { get; private set; }

        public PlayerInvincibleEvent(bool isInvincible)
        {
            IsInvincible = isInvincible;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 점프 공격 이벤트 (State → Player, 사운드/이펙트용)
    public class PlayerJumpAttackEvent : IEvent
    {
        public int Direction { get; private set; }

        public PlayerJumpAttackEvent(int direction)
        {
            Direction = direction;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 백스텝 이벤트 (State → Player, 사운드/이펙트용)
    public class PlayerBackstepEvent : IEvent
    {
        public int Direction { get; private set; }

        public PlayerBackstepEvent(int direction)
        {
            Direction = direction;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 플레이어 점프 찍기 공격 이벤트 (State → Player, 사운드/이펙트용)
    public class PlayerDiveAttackEvent : IEvent
    {
        public int Direction { get; private set; }

        public PlayerDiveAttackEvent(int direction)
        {
            Direction = direction;
        }

        public Type GetPublishType()
        {
            return typeof(PlayerStateBase);
        }
    }

    // 방어막 획득 이벤트 (ShieldItem → Player)
    public class ShieldCollectedEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(ShieldItem);
        }
    }

    // 방어막 파괴 이벤트 (Player → UI 등)
    public class ShieldBrokenEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerUnit);
        }
    }
}
