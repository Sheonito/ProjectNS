using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.FSM;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 상태 기본 클래스 (StateMachine 참조 없음, EventBus로 상태 전환 요청)
    public abstract class PlayerStateBase : IState
    {
        public OnEnter onEnter { get; set; }
        public OnExecute onExecute { get; set; }
        public OnExit onExit { get; set; }

        protected UIInputAction _inputAction;

        protected PlayerStateBase()
        {
            _inputAction = UIInputAction.Instance;
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

        // 입력 수집 (공통)
        protected float GetHorizontalInput()
        {
            return _inputAction.Player.Move.ReadValue<Vector2>().x;
        }

        protected bool IsJumpPressed()
        {
            return _inputAction.GetButtonDown(InputActionType.Jump);
        }

        protected bool IsJumpHeld()
        {
            return _inputAction.GetButton(InputActionType.Jump);
        }

        protected bool IsAttackPressed()
        {
            return _inputAction.GetButtonDown(InputActionType.Attack);
        }

        protected bool IsDashAttackPressed()
        {
            return _inputAction.GetButtonDown(InputActionType.DashAttack);
        }

        // 상태 전환 요청 (EventBus 사용, StateMachine 직접 참조 없음)
        protected void RequestStateChange(PlayerStateType targetState)
        {
            EventBus.Publish(this, new PlayerChangeStateRequestEvent(targetState));
        }

        // 점프 이벤트 발행
        protected void PublishJumpEvent()
        {
            EventBus.Publish(this, new PlayerJumpEvent());
        }
    }
}
