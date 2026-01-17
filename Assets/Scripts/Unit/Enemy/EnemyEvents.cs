using System;
using Percent111.ProjectNS.Event;

namespace Percent111.ProjectNS.Enemy
{
    // 적 상태 전환 요청 이벤트 (State → StateMachine)
    public class EnemyChangeStateRequestEvent : IEvent
    {
        public EnemyStateType RequestedState { get; private set; }
        public EnemyMovement Owner { get; private set; }

        public EnemyChangeStateRequestEvent(EnemyStateType requestedState, EnemyMovement owner)
        {
            RequestedState = requestedState;
            Owner = owner;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyStateBase);
        }
    }

    // 적 강제 상태 전환 요청 이벤트 (Enemy → StateMachine)
    public class EnemyForceStateChangeEvent : IEvent
    {
        public EnemyStateType RequestedState { get; private set; }
        public EnemyMovement Owner { get; private set; }

        public EnemyForceStateChangeEvent(EnemyStateType requestedState, EnemyMovement owner)
        {
            RequestedState = requestedState;
            Owner = owner;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyUnit);
        }
    }

    // 적 상태 변경 완료 이벤트 (StateMachine → Enemy)
    public class EnemyStateChangedEvent : IEvent
    {
        public EnemyStateType PreviousState { get; private set; }
        public EnemyStateType CurrentState { get; private set; }
        public EnemyMovement Owner { get; private set; }

        public EnemyStateChangedEvent(EnemyStateType previousState, EnemyStateType currentState, EnemyMovement owner)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Owner = owner;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyStateMachine);
        }
    }

    // 적 플레이어 발견 이벤트 (Movement → State)
    public class EnemyPlayerDetectedEvent : IEvent
    {
        public bool IsDetected { get; private set; }

        public EnemyPlayerDetectedEvent(bool isDetected)
        {
            IsDetected = isDetected;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyMovement);
        }
    }

    // 적 공격 이벤트 (State → Enemy)
    public class EnemyAttackEvent : IEvent
    {
        public int Damage { get; private set; }
        public EnemyMovement Owner { get; private set; }

        public EnemyAttackEvent(int damage, EnemyMovement owner)
        {
            Damage = damage;
            Owner = owner;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyStateBase);
        }
    }

    // 적 데미지 이벤트 (Enemy → State)
    public class EnemyDamageEvent : IEvent
    {
        public int Damage { get; private set; }
        public int CurrentHp { get; private set; }

        public EnemyDamageEvent(int damage, int currentHp)
        {
            Damage = damage;
            CurrentHp = currentHp;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyUnit);
        }
    }

    // 적 사망 이벤트 (Enemy → State)
    public class EnemyDeathEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(EnemyUnit);
        }
    }

    // 적 사망 완료 이벤트 (State → Enemy, Fade 시작 트리거)
    public class EnemyDeathCompleteEvent : IEvent
    {
        public EnemyMovement Owner { get; private set; }

        public EnemyDeathCompleteEvent(EnemyMovement owner)
        {
            Owner = owner;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyStateBase);
        }
    }

    // 적 Pool 반환 요청 이벤트 (Enemy → StageManager)
    public class EnemyReturnToPoolEvent : IEvent
    {
        public EnemyUnit Enemy { get; private set; }

        public EnemyReturnToPoolEvent(EnemyUnit enemy)
        {
            Enemy = enemy;
        }

        public Type GetPublishType()
        {
            return typeof(EnemyUnit);
        }
    }
}
