using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 원거리 공격 상태 (투사체 발사)
    public class EnemyRangedAttackState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private readonly EnemyAnimator _animator;
        private readonly ProjectilePool _projectilePool;
        private readonly ProjectileSettings _projectileSettings;
        private readonly Transform _firePoint;
        private float _attackTimer;
        private float _attackDuration;
        private float _fireTiming;
        private bool _hasFired;

        public EnemyRangedAttackState(
            EnemyMovement movement, 
            EnemyStateSettings settings, 
            EnemyAnimator animator,
            ProjectilePool projectilePool,
            ProjectileSettings projectileSettings,
            Transform firePoint) : base(movement)
        {
            _settings = settings;
            _animator = animator;
            _projectilePool = projectilePool;
            _projectileSettings = projectileSettings;
            _firePoint = firePoint;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasFired = false;
            _movement.Stop();
            _movement.LookAtPlayer();
            _movement.StartAttackCooldown();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _attackDuration = _settings.attackTargetDuration;
            _fireTiming = _attackDuration * _settings.attackHitTimingRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(EnemyStateType.Attack);
            float autoSpeedFactor = baseAnimLength / _attackDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;
            
            // 발사 타이밍
            if (!_hasFired && _attackTimer >= _fireTiming)
            {
                _hasFired = true;
                FireProjectile();
            }

            // 공격 완료
            if (_attackTimer >= _attackDuration)
            {
                // 플레이어가 여전히 탐지 중이면 추적
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

        // 투사체 발사
        private void FireProjectile()
        {
            if (_projectilePool == null)
                return;

            // 발사 위치 (FirePoint가 없으면 적 자신의 위치 사용)
            Vector2 firePosition = _firePoint != null ? (Vector2)_firePoint.position : _movement.GetPosition();

            // 플레이어 위치
            Vector2 playerPos = _movement.GetPlayerPosition();
            if (playerPos == Vector2.zero)
                return;

            // 발사 방향 결정 (플레이어가 땅에 있으면 수평 발사)
            Vector2 direction;
            if (_movement.IsPlayerGrounded())
            {
                // 수평 방향으로만 발사
                float horizontalDir = Mathf.Sign(playerPos.x - firePosition.x);
                direction = new Vector2(horizontalDir, 0);
            }
            else
            {
                // 플레이어를 향해 발사
                direction = (playerPos - firePosition).normalized;
            }

            // 투사체 스폰 및 초기화
            Projectile projectile = _projectilePool.Spawn(firePosition);
            projectile.Initialize(
                _settings.attackDamage, 
                _projectileSettings.speed, 
                _projectileSettings.lifeTime, 
                direction);
        }

        public override void Exit()
        {
            base.Exit();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
