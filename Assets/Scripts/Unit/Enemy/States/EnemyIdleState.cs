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
            _movement.UpdatePhysics();

            // 플레이어 발견 시 추적
            if (_movement.IsPlayerDetected())
            {
                RequestStateChange(EnemyStateType.Chase);
                return;
            }

            // 일정 시간 후 순찰
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _settings.idleDuration)
            {
                RequestStateChange(EnemyStateType.Patrol);
                return;
            }
        }
    }
}
