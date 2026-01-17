using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 공격 상태
    public class EnemyAttackState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private float _attackTimer;
        private bool _hasAttacked;

        public EnemyAttackState(EnemyMovement movement, EnemyStateSettings settings) : base(movement)
        {
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasAttacked = false;
            _movement.Stop();
            _movement.LookAtPlayer();
            _movement.StartAttackCooldown();
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;

            // 공격 타이밍 (중간쯤)
            if (!_hasAttacked && _attackTimer >= _settings.attackDuration * 0.5f)
            {
                _hasAttacked = true;
                PublishAttackEvent(_settings.attackDamage);
            }

            // 공격 완료
            if (_attackTimer >= _settings.attackDuration)
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

            _movement.UpdatePhysics();
        }
    }
}
