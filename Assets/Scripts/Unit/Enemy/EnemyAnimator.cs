using System.Collections.Generic;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 애니메이션 재생 담당 (EventBus를 통해 상태 변경 수신)
    public class EnemyAnimator
    {
        // 애니메이션 이름 상수
        private static class AnimationNames
        {
            public const string Idle = "IDLE";
            public const string Move = "MOVE";
            public const string Attack = "ATTACK";
            public const string Damaged = "DAMAGED";
            public const string Death = "DEATH";
        }

        private readonly Animator _animator;
        private readonly EnemyMovement _ownerMovement;
        private readonly Dictionary<EnemyStateType, string> _stateToAnimation;

        // 생성자
        public EnemyAnimator(Animator animator, EnemyMovement ownerMovement)
        {
            _animator = animator;
            _ownerMovement = ownerMovement;
            _stateToAnimation = new Dictionary<EnemyStateType, string>
            {
                { EnemyStateType.Idle, AnimationNames.Idle },
                { EnemyStateType.Patrol, AnimationNames.Move },
                { EnemyStateType.Chase, AnimationNames.Move },
                { EnemyStateType.Attack, AnimationNames.Attack },
                { EnemyStateType.Damaged, AnimationNames.Damaged },
                { EnemyStateType.Death, AnimationNames.Death }
            };
        }

        // 이벤트 구독
        public void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyStateChangedEvent>(OnEnemyStateChanged);
        }

        // 이벤트 구독 해제
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyStateChangedEvent>(OnEnemyStateChanged);
        }

        // 상태 변경 이벤트 핸들러
        private void OnEnemyStateChanged(EnemyStateChangedEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _ownerMovement)
                return;

            PlayAnimation(evt.CurrentState);
        }

        // 상태에 따른 애니메이션 재생
        public void PlayAnimation(EnemyStateType stateType)
        {
            if (_stateToAnimation.TryGetValue(stateType, out string animationName))
            {
                _animator.Play(animationName);
            }
        }

        // 특정 애니메이션 직접 재생
        public void PlayAnimation(string animationName)
        {
            _animator.Play(animationName);
        }

        // 현재 애니메이션 재생 완료 여부
        public bool IsCurrentAnimationFinished()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1.0f;
        }
    }
}
