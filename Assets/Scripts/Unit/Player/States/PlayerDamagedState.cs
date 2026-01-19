using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 피격 상태
    public class PlayerDamagedState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _damagedTimer;
        private float _damagedDuration;

        public PlayerDamagedState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _damagedTimer = 0;

            // 이동 중지
            _movement.SetHorizontalInput(0);
            _movement.SetVelocity(Vector2.zero);

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _damagedDuration = _settings.damaged.targetDuration;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.Damaged);
            if (baseAnimLength > 0 && _damagedDuration > 0)
            {
                float autoSpeedFactor = baseAnimLength / _damagedDuration;
                _animator.SetAnimationSpeed(autoSpeedFactor);
            }
        }

        public override void Execute()
        {
            base.Execute();

            _damagedTimer += Time.deltaTime;

            // 피격 경직 완료
            if (_damagedTimer >= _damagedDuration)
            {
                float horizontalInput = GetHorizontalInput();

                if (!_movement.IsGrounded())
                {
                    RequestStateChange(PlayerStateType.Jump);
                }
                else if (Mathf.Abs(horizontalInput) > 0.01f)
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

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();
            _movement.UpdatePhysics();
        }

        public override void Exit()
        {
            base.Exit();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
