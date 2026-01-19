using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 사망 상태
    public class EnemyDeathState : EnemyStateBase
    {
        private readonly EnemyStateSettings _settings;
        private readonly EnemyAnimator _animator;
        private float _deathDuration;

        public EnemyDeathState(EnemyMovement movement, EnemyStateSettings settings, EnemyAnimator animator) : base(movement)
        {
            _settings = settings;
            _animator = animator;
        }

        public override async void Enter()
        {
            base.Enter();
            _movement.Stop();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _deathDuration = _settings.death.targetDuration;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(EnemyStateType.Death);
            float autoSpeedFactor = baseAnimLength / _deathDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            await UniTask.WaitForSeconds(_deathDuration);
            PublishDeathCompleteEvent();
        }

        public override void Exit()
        {
            base.Exit();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}