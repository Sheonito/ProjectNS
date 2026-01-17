using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 사망 상태
    public class EnemyDeathState : EnemyStateBase
    {
        private float _deathDuration;
        private float _deathTimer;
        private bool _isDeathComplete;

        public EnemyDeathState(EnemyMovement movement, float deathDuration = 1f) : base(movement)
        {
            _deathDuration = deathDuration;
        }

        public override void Enter()
        {
            base.Enter();
            _deathTimer = 0;
            _isDeathComplete = false;
            _movement.Stop();
        }

        public override void Execute()
        {
            base.Execute();

            if (_isDeathComplete) return;

            _deathTimer += Time.deltaTime;

            // 사망 애니메이션 완료 후 처리
            if (_deathTimer >= _deathDuration)
            {
                _isDeathComplete = true;
                // 오브젝트 비활성화 또는 풀링 반환은 Enemy에서 처리
            }
        }

        // 사망 완료 여부
        public bool IsDeathComplete() => _isDeathComplete;
    }
}
