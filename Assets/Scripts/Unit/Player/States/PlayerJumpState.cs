using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 점프/낙하 상태 (Movement 필요)
    public class PlayerJumpState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private bool _hasJumped;

        public PlayerJumpState(PlayerMovement movement, PlayerStateSettings settings) : base()
        {
            _movement = movement;
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();

            _hasJumped = false;

            // 점프 입력이 있고 점프 가능하면 점프 실행
            if (_movement.CanJump())
            {
                _movement.Jump();
                _hasJumped = true;
                
                // 점프 이벤트 발행 (사운드, 이펙트 등 처리용)
                PublishJumpEvent();
            }
        }

        public override void Execute()
        {
            base.Execute();

            float horizontalInput = GetHorizontalInput();

            _movement.SetHorizontalInput(horizontalInput);
            _movement.UpdatePhysics();

            // 점프 버튼을 떼면 낮은 점프
            if (!IsJumpHeld())
            {
                _movement.CutJump();
            }

            // 공격 입력 (공중에서도 가능)
            if (IsAttackPressed())
            {
                RequestStateChange(PlayerStateType.Attack);
                return;
            }

            // 착지하면 상태 전환
            if (_movement.IsGrounded() && _movement.GetVelocity().y <= 0)
            {
                if (Mathf.Abs(horizontalInput) > 0.01f)
                {
                    RequestStateChange(PlayerStateType.Move);
                }
                else
                {
                    RequestStateChange(PlayerStateType.Idle);
                }
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
