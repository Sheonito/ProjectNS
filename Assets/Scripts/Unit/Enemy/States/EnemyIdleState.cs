using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 대기 상태
    public class EnemyIdleState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private float _idleTimer;

        public EnemyIdleState(EnemyMovement movement, EnemyStateSettings settings) : base(movement)
        {
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _idleTimer = 0;
            _movement.Stop();
        }

        public override void Execute()
        {
            base.Execute();

            _movement.UpdateDetection();

            // 플레이어 발견 시
            if (_movement.IsPlayerDetected())
            {
                // 공격 가능하면 공격
                if (_movement.CanAttack())
                {
                    RequestStateChange(EnemyStateType.Attack);
                    return;
                }

                // 공격 범위 내에 있으면 대기 (쿨타임 중)
                if (_movement.IsInAttackRange())
                {
                    _movement.LookAtPlayer();
                    return;
                }

                // 공격 범위 밖이면 추적
                RequestStateChange(EnemyStateType.Chase);
                return;
            }

            // 일정 시간 후 순찰
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _settings.idle.duration)
            {
                RequestStateChange(EnemyStateType.Patrol);
                return;
            }
        }

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();
            _movement.UpdatePhysics();
        }
    }
}
