using Percent111.ProjectNS.DI;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 원거리 적 클래스
    public class RangedEnemyUnit : EnemyUnit
    {
        [Header("Ranged Attack")]
        [SerializeField] private ProjectileSettings _projectileSettings;
        [SerializeField] private Transform _firePoint;

        private ProjectilePool _projectilePool;

        protected override void Awake()
        {
            // DI를 통해 ProjectilePool 주입
            _projectilePool = DIResolver.Resolve<ProjectilePool>();
            
            base.Awake();
        }

        // 상태 머신 초기화 (원거리 전용 AttackState 사용)
        protected override void CreateStateMachine()
        {
            _stateMachine = new EnemyStateMachine(_movement);

            EnemyIdleState idleState = new EnemyIdleState(_movement, _stateSettings);
            EnemyPatrolState patrolState = new EnemyPatrolState(_movement, _stateSettings);
            EnemyChaseState chaseState = new EnemyChaseState(_movement);
            EnemyRangedAttackState attackState = new EnemyRangedAttackState(
                _movement, 
                _stateSettings, 
                _enemyAnimator,
                _projectilePool,
                _projectileSettings,
                _firePoint);
            EnemyDamagedState damagedState = new EnemyDamagedState(_movement, _stateSettings, _enemyAnimator);
            EnemyDeathState deathState = new EnemyDeathState(_movement, _stateSettings, _enemyAnimator);

            _stateMachine.RegisterState(EnemyStateType.Idle, idleState);
            _stateMachine.RegisterState(EnemyStateType.Patrol, patrolState);
            _stateMachine.RegisterState(EnemyStateType.Chase, chaseState);
            _stateMachine.RegisterState(EnemyStateType.Attack, attackState);
            _stateMachine.RegisterState(EnemyStateType.Damaged, damagedState);
            _stateMachine.RegisterState(EnemyStateType.Death, deathState);

            _stateMachine.InitWithState(EnemyStateType.Idle);
        }
    }
}
