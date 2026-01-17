using System.Collections.Generic;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.FSM;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태를 정의하는 열거형
    public enum PlayerStateType
    {
        Idle,
        Move,
        Jump,
        Attack,
        Damaged,
        Death
    }

    // 플레이어 전용 상태 머신 (State로부터 이벤트 구독, State 직접 참조 없음)
    public class PlayerStateMachine : StateMachine
    {
        private Dictionary<PlayerStateType, IState> _states;
        private PlayerStateType _currentStateType;

        public PlayerStateMachine()
        {
            _states = new Dictionary<PlayerStateType, IState>();
            _currentStateType = PlayerStateType.Idle;
        }

        // 이벤트 구독 (Player에서 호출)
        public void SubscribeEvents()
        {
            EventBus.Subscribe<PlayerChangeStateRequestEvent>(OnChangeStateRequest);
        }

        // 이벤트 구독 해제 (Player에서 호출)
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<PlayerChangeStateRequestEvent>(OnChangeStateRequest);
        }

        // 상태 전환 요청 이벤트 핸들러
        private void OnChangeStateRequest(PlayerChangeStateRequestEvent evt)
        {
            ChangeStateInternal(evt.RequestedState);
        }

        // 상태 등록
        public void RegisterState(PlayerStateType stateType, IState state)
        {
            _states[stateType] = state;
        }

        // 초기 상태 설정
        public void InitWithState(PlayerStateType stateType)
        {
            if (_states.TryGetValue(stateType, out IState state))
            {
                _currentStateType = stateType;
                Init(state);
            }
        }

        // 내부 상태 변경 (이벤트 발행)
        private void ChangeStateInternal(PlayerStateType stateType)
        {
            if (_states.TryGetValue(stateType, out IState state))
            {
                PlayerStateType previousStateType = _currentStateType;
                _currentStateType = stateType;
                ChangeState(state);
                
                // 상태 변경 이벤트 발행
                EventBus.Publish(this, new PlayerStateChangedEvent(previousStateType, stateType));
            }
        }

        // 현재 상태 타입 반환
        public PlayerStateType GetCurrentStateType()
        {
            return _currentStateType;
        }
    }
}
