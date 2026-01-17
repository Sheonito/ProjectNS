using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 대기 상태 (Movement 필요)
    public class PlayerIdleState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;

        public PlayerIdleState(PlayerMovement movement, PlayerStateSettings settings) : base()
        {
            _movement = movement;
            _settings = settings;
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

            // 대시공격 입력 (전용 버튼, 쿨타임 체크는 DashAttackState에서)
            if (IsDashAttackPressed())
            {
                RequestStateChange(PlayerStateType.DashAttack);
                return;
            }

            // 공격 입력 - 마우스 Y 위치에 따라 자동 전환 (캐릭터 중심 기준)
            if (IsAttackPressed())
            {
                Vector2 playerPos = _movement.GetPosition();
                MouseVerticalPosition mousePos = GetMouseVerticalPosition(playerPos, _settings.characterCenterOffset);

                if (mousePos == MouseVerticalPosition.Above)
                {
                    // 마우스가 위에 있으면 대각선 점프 공격
                    RequestStateChange(PlayerStateType.JumpAttack);
                }
                else
                {
                    // 마우스가 아래에 있으면 슬래시 공격 (살짝 대시 포함)
                    RequestStateChange(PlayerStateType.Attack);
                }
                return;
            }

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
