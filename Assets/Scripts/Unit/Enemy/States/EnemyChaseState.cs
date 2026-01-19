namespace Percent111.ProjectNS.Enemy
{
    // 적 추적 상태
    public class EnemyChaseState : EnemyStateBase
    {
        public EnemyChaseState(EnemyMovement movement) : base(movement)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Execute()
        {
            base.Execute();

            _movement.UpdateDetection();

            // 플레이어 놓침 - 대기로 전환
            if (!_movement.IsPlayerDetected())
            {
                RequestStateChange(EnemyStateType.Idle);
                return;
            }

            // 공격 가능 시 공격
            if (_movement.CanAttack())
            {
                RequestStateChange(EnemyStateType.Attack);
                return;
            }

            // 공격 범위 내에 있으면 Idle로 전환 (쿨타임 대기)
            if (_movement.IsInAttackRange())
            {
                RequestStateChange(EnemyStateType.Idle);
                return;
            }

            // 플레이어 방향으로 이동 입력 설정
            _movement.MoveTowardsPlayer();
        }

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();
            _movement.UpdatePhysics();
        }

        public override void Exit()
        {
            base.Exit();
            _movement.Stop();
        }
    }
}
