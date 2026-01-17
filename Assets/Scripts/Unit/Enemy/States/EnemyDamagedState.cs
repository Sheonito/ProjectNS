using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 피격 상태
    public class EnemyDamagedState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private float _damagedTimer;

        public EnemyDamagedState(EnemyMovement movement, EnemyStateSettings settings) : base(movement)
        {
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _damagedTimer = 0;
            _movement.Stop();
        }

        public override void Execute()
        {
            base.Execute();

            _damagedTimer += Time.deltaTime;

            // 피격 경직 완료
            if (_damagedTimer >= _settings.damagedDuration)
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

            _movement.UpdatePhysics();
        }
    }
}
