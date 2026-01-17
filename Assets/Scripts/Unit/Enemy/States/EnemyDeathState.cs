using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 사망 상태
    public class EnemyDeathState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private float _deathTimer;
        private bool _isDeathComplete;

        public EnemyDeathState(EnemyMovement movement, EnemyStateSettings settings) : base(movement)
        {
            _settings = settings;
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
            if (_deathTimer >= _settings.deathDuration)
            {
                _isDeathComplete = true;
                // 오브젝트 비활성화 또는 풀링 반환은 Enemy에서 처리
            }
        }

        // 사망 완료 여부
        public bool IsDeathComplete() => _isDeathComplete;
    }
}
