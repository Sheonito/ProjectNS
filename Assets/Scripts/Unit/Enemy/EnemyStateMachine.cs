using System.Collections.Generic;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.FSM;

namespace Percent111.ProjectNS.Enemy
{
    // 적 상태를 정의하는 열거형
    public enum EnemyStateType
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Damaged,
        Death
    }

    // 적 전용 상태 머신 (State로부터 이벤트 구독)
    public class EnemyStateMachine : StateMachine
    {
        private Dictionary<EnemyStateType, IState> _states;
        private EnemyMovement _ownerMovement;
        public EnemyStateType CurrentStateType { get; private set; }

        public EnemyStateMachine(EnemyMovement ownerMovement)
        {
            _states = new Dictionary<EnemyStateType, IState>();
            _ownerMovement = ownerMovement;
            CurrentStateType = EnemyStateType.Idle;
        }

        // 이벤트 구독 (Enemy에서 호출)
        public void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyChangeStateRequestEvent>(OnChangeStateRequest);
        }

        // 이벤트 구독 해제 (Enemy에서 호출)
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyChangeStateRequestEvent>(OnChangeStateRequest);
        }

        // 상태 전환 요청 이벤트 핸들러 (State에서 요청)
        private void OnChangeStateRequest(EnemyChangeStateRequestEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _ownerMovement)
                return;

            ChangeState(evt.RequestedState);
        }

        // 상태 등록
        public void RegisterState(EnemyStateType stateType, IState state)
        {
            _states[stateType] = state;
        }

        // 초기 상태 설정
        public void InitWithState(EnemyStateType stateType)
        {
            if (_states.TryGetValue(stateType, out IState state))
            {
                CurrentStateType = stateType;
                Init(state);
            }
        }

        // 상태 변경 (외부에서 직접 호출 가능)
        public void ChangeState(EnemyStateType stateType)
        {
            if (_states.TryGetValue(stateType, out IState state))
            {
                EnemyStateType previousStateType = CurrentStateType;
                CurrentStateType = stateType;
                ChangeState(state);

                // 상태 변경 이벤트 발행 (소유자 정보 포함)
                EventBus.Publish(this, new EnemyStateChangedEvent(previousStateType, stateType, _ownerMovement));
            }
        }
    }
}
