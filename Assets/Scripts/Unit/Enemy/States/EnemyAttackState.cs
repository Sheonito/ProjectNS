using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 공격 상태
    public class EnemyAttackState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private readonly EnemyAnimator _animator;
        private float _attackTimer;
        private float _attackDuration;
        private float _hitTiming;
        private bool _hasAttacked;

        public EnemyAttackState(EnemyMovement movement, EnemyStateSettings settings, EnemyAnimator animator) : base(movement)
        {
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasAttacked = false;
            _movement.Stop();
            _movement.LookAtPlayer();
            _movement.StartAttackCooldown();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _attackDuration = _settings.attack.targetDuration;
            _hitTiming = _attackDuration * _settings.attack.hitTimingRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(EnemyStateType.Attack);
            float autoSpeedFactor = baseAnimLength / _attackDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;

            // 공격 타이밍
            if (!_hasAttacked && _attackTimer >= _hitTiming)
            {
                _hasAttacked = true;
                PublishAttackEvent(_settings.attack.damage);
            }

            // 공격 완료
            if (_attackTimer >= _attackDuration)
            {
                // 플레이어가 여전히 범위 내이고 탐지 중이면 추적
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
