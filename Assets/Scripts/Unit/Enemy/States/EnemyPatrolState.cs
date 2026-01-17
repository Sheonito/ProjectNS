using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 순찰 상태
    public class EnemyPatrolState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private int _patrolDirection = 1;
        private float _patrolTimer;

        public EnemyPatrolState(EnemyMovement movement, EnemyStateSettings settings) : base(movement)
        {
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _patrolTimer = 0;
            // 랜덤 방향 또는 이전 방향 반대로
            _patrolDirection = Random.value > 0.5f ? 1 : -1;
        }

        public override void Execute()
        {
            base.Execute();

            _movement.UpdateDetection();

            // 플레이어 발견 시 추적
            if (_movement.IsPlayerDetected())
            {
                RequestStateChange(EnemyStateType.Chase);
                return;
            }

            // 순찰 이동
            _movement.Patrol(_patrolDirection);
            _movement.UpdatePhysics();

            // 일정 시간 후 대기
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= _settings.patrolDuration)
            {
                RequestStateChange(EnemyStateType.Idle);
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
            _movement.Stop();
        }
    }
}
