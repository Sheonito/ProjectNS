using Cysharp.Threading.Tasks;
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

        public override async void Enter()
        {
            base.Enter();
            _deathTimer = 0;
            _isDeathComplete = false;
            _movement.Stop();

            _deathTimer += Time.deltaTime;

            await UniTask.WaitForSeconds(_settings.deathDuration);
            _isDeathComplete = true;
            PublishDeathCompleteEvent();
        }
    }
}