using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 사망 상태
    public class PlayerDeathState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _deathTimer;
        private float _deathDuration;
        private bool _isDeathComplete;

        public PlayerDeathState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _deathTimer = 0;
            _isDeathComplete = false;

            // 이동 중지
            _movement.SetHorizontalInput(0);
            _movement.SetVelocity(Vector2.zero);

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _deathDuration = _settings.deathTargetDuration;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.Death);
            if (baseAnimLength > 0 && _deathDuration > 0)
            {
                float autoSpeedFactor = baseAnimLength / _deathDuration;
                _animator.SetAnimationSpeed(autoSpeedFactor);
            }
        }

        public override void Execute()
        {
            base.Execute();

            _deathTimer += Time.deltaTime;

            // 물리 업데이트 (중력 적용)
            _movement.UpdatePhysics();

            // 사망 애니메이션 완료 체크
            if (!_isDeathComplete && _deathTimer >= _deathDuration)
            {
                _isDeathComplete = true;
                // 사망 상태는 유지 (게임 오버 처리는 외부에서)
            }
        }

        public override void Exit()
        {
            base.Exit();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
