using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 이동 상태 (Movement 필요)
    public class PlayerMoveState : PlayerStateBase
    {
        private PlayerMovement _movement;

        public PlayerMoveState(PlayerMovement movement) : base()
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

            float horizontalInput = GetHorizontalInput();

            _movement.SetHorizontalInput(horizontalInput);
            _movement.UpdatePhysics();

            // 이동 입력 없으면 Idle 상태로 전환
            if (Mathf.Abs(horizontalInput) < 0.01f && _movement.IsGrounded())
            {
                RequestStateChange(PlayerStateType.Idle);
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
