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
        private EnemyStateType _currentStateType;

        public EnemyStateMachine()
        {
            _states = new Dictionary<EnemyStateType, IState>();
            _currentStateType = EnemyStateType.Idle;
        }

        // 이벤트 구독 (Enemy에서 호출)
        public void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyChangeStateRequestEvent>(OnChangeStateRequest);
            EventBus.Subscribe<EnemyForceStateChangeEvent>(OnForceStateChange);
        }

        // 이벤트 구독 해제 (Enemy에서 호출)
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyChangeStateRequestEvent>(OnChangeStateRequest);
            EventBus.Unsubscribe<EnemyForceStateChangeEvent>(OnForceStateChange);
        }

        // 상태 전환 요청 이벤트 핸들러 (State에서 요청)
        private void OnChangeStateRequest(EnemyChangeStateRequestEvent evt)
        {
            ChangeStateInternal(evt.RequestedState);
        }

        // 강제 상태 전환 이벤트 핸들러 (Enemy에서 요청)
        private void OnForceStateChange(EnemyForceStateChangeEvent evt)
        {
            ChangeStateInternal(evt.RequestedState);
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
                _currentStateType = stateType;
                Init(state);
            }
        }

        // 내부 상태 변경 (이벤트 발행)
        private void ChangeStateInternal(EnemyStateType stateType)
        {
            if (_states.TryGetValue(stateType, out IState state))
            {
                EnemyStateType previousStateType = _currentStateType;
                _currentStateType = stateType;
                ChangeState(state);

                // 상태 변경 이벤트 발행
                EventBus.Publish(this, new EnemyStateChangedEvent(previousStateType, stateType));
            }
        }

        // 현재 상태 타입 반환
        public EnemyStateType GetCurrentStateType()
        {
            return _currentStateType;
        }
    }
}
