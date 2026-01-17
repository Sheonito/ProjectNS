using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.FSM;

namespace Percent111.ProjectNS.Enemy
{
    // 적 상태 기본 클래스 (StateMachine 참조 없음, EventBus로 상태 전환 요청)
    public abstract class EnemyStateBase : IState
    {
        public OnEnter onEnter { get; set; }
        public OnExecute onExecute { get; set; }
        public OnExit onExit { get; set; }

        protected EnemyMovement _movement;

        // 생성자
        protected EnemyStateBase(EnemyMovement movement)
        {
            _movement = movement;
        }

        // Movement가 필요 없는 State용 생성자
        protected EnemyStateBase()
        {
        }

        public virtual void Enter()
        {
            onEnter?.Invoke();
        }

        public virtual void Execute()
        {
            onExecute?.Invoke();
        }

        public virtual void Exit()
        {
            onExit?.Invoke();
        }

        // 상태 전환 요청 (EventBus 사용, 소유자 정보 포함)
        protected void RequestStateChange(EnemyStateType targetState)
        {
            EventBus.Publish(this, new EnemyChangeStateRequestEvent(targetState, _movement));
        }

        // 공격 이벤트 발행 (소유자 정보 포함)
        protected void PublishAttackEvent(int damage)
        {
            EventBus.Publish(this, new EnemyAttackEvent(damage, _movement));
        }
    }
}
