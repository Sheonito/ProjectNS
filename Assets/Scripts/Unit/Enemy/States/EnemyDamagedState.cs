using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 피격 상태
    public class EnemyDamagedState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private readonly EnemyAnimator _animator;
        private float _damagedTimer;
        private float _damagedDuration;

        public EnemyDamagedState(EnemyMovement movement, EnemyStateSettings settings, EnemyAnimator animator) : base(movement)
        {
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _damagedTimer = 0;
            _movement.Stop();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _damagedDuration = _settings.damagedTargetDuration;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(EnemyStateType.Damaged);
            float autoSpeedFactor = baseAnimLength / _damagedDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);
        }

        public override void Execute()
        {
            base.Execute();

            _damagedTimer += Time.deltaTime;

            // 피격 경직 완료
            if (_damagedTimer >= _damagedDuration)
            {
                // 플레이어 탐지 중이면 추적, 아니면 대기
                _movement.UpdateDetection();
                if (_movement.IsPlayerDetected())
                {
                    RequestStateChange(EnemyStateType.Chase);
                }
                else
                {
                    RequestStateChange(EnemyStateType.Idle);
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
