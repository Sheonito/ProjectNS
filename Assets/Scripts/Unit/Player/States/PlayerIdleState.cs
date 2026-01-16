using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 대기 상태 (Movement 필요)
    public class PlayerIdleState : PlayerStateBase
    {
        private PlayerMovement _movement;

        public PlayerIdleState(PlayerMovement movement) : base()
        {
            _movement = movement;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Execute()
        {
            base.Execute();

            _movement.UpdatePhysics();

            float horizontalInput = GetHorizontalInput();

            // 이동 입력 시 Move 상태로 전환
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                RequestStateChange(PlayerStateType.Move);
                return;
            }

            // 점프 입력 시 Jump 상태로 전환
            if (IsJumpPressed() && _movement.CanJump())
            {
                RequestStateChange(PlayerStateType.Jump);
                return;
            }

            // 공중에 있으면 Jump 상태로 전환
            if (!_movement.IsGrounded())
            {
                RequestStateChange(PlayerStateType.Jump);
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
