using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.FSM;
using UnityEngine;

// 마우스 Y 위치에 따른 공격 타입
public enum MouseVerticalPosition
{
    Above,  // 플레이어 위 (점프 공격)
    Below   // 플레이어 아래 (대시 공격)
}

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

        // 무적 상태 변경 이벤트 발행
        protected void SetInvincible(bool isInvincible)
        {
            EventBus.Publish(this, new PlayerInvincibleEvent(isInvincible));
        }

        // 마우스 월드 위치 반환
        protected Vector2 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        // 플레이어 위치 기준 마우스 방향 벡터 반환 (정규화)
        protected Vector2 GetMouseDirection(Vector2 playerPosition)
        {
            Vector2 mouseWorld = GetMouseWorldPosition();
            Vector2 direction = (mouseWorld - playerPosition).normalized;
            return direction;
        }

        // 마우스가 플레이어 기준 위/아래 어디에 있는지 반환 (캐릭터 중심 오프셋 적용)
        protected MouseVerticalPosition GetMouseVerticalPosition(Vector2 playerPosition, float centerOffset = 0f)
        {
            Vector2 mouseWorld = GetMouseWorldPosition();
            float centerY = playerPosition.y + centerOffset;
            return mouseWorld.y > centerY ? MouseVerticalPosition.Above : MouseVerticalPosition.Below;
        }

        // 마우스 방향에 따른 X 방향 반환 (-1: 왼쪽, 1: 오른쪽)
        protected int GetMouseHorizontalDirection(Vector2 playerPosition)
        {
            Vector2 mouseWorld = GetMouseWorldPosition();
            return mouseWorld.x > playerPosition.x ? 1 : -1;
        }
    }
}
