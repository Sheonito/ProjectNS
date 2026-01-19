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

            float horizontalInput = GetHorizontalInput();

            // 백스텝 입력 (쿨타임 체크 포함)
            if (IsBackstepPressed() && !PlayerBackstepState.IsOnCooldownStatic())
            {
                RequestStateChange(PlayerStateType.Backstep);
                return;
            }

            // 대시공격 입력 (쿨타임 체크 포함)
            if (IsDashAttackPressed() && !PlayerDashAttackState.IsOnCooldownStatic())
            {
                RequestStateChange(PlayerStateType.DashAttack);
                return;
            }

            // 공격 입력
            if (IsAttackPressed())
            {
                Vector2 playerPos = _movement.GetPosition();

                // 적이 공격 범위 내에 있으면 제자리 공격 (쿨타임 체크)
                if (IsEnemyInAttackRange(playerPos, _settings.attackRange, _settings.enemyLayer))
                {
                    if (!PlayerAttackState.IsOnCooldownStatic())
                    {
                        RequestStateChange(PlayerStateType.Attack);
                    }
                }
                else
                {
                    // 적이 없으면 마우스 Y 위치에 따라 자동 전환 (캐릭터 중심 기준)
                    MouseVerticalPosition mousePos = GetMouseVerticalPosition(playerPos, _settings.characterCenterOffset);

                    if (mousePos == MouseVerticalPosition.Above)
                    {
                        // 마우스가 위에 있으면 점프 공격만 시도 (쿨타임이면 무시, 폴백 없음)
                        if (!PlayerJumpAttackState.IsOnCooldownStatic())
                        {
                            RequestStateChange(PlayerStateType.JumpAttack);
                        }
                    }
                    else
                    {
                        // 마우스가 아래에 있으면 슬래시 공격만 시도 (쿨타임이면 무시, 폴백 없음)
                        if (!PlayerAttackState.IsOnCooldownStatic())
                        {
                            RequestStateChange(PlayerStateType.Attack);
                        }
                    }
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

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();
            _movement.UpdatePhysics();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
