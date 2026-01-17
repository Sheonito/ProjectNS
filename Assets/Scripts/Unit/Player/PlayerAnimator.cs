using System.Collections.Generic;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 애니메이션 재생 담당 (EventBus를 통해 상태 변경 수신)
    public class PlayerAnimator
    {
        // 애니메이션 이름 상수 (Animator Controller 기준)
        private static class AnimationNames
        {
            public const string Idle = "IDLE";
            public const string Move = "MOVE";
            public const string Attack = "ATTACK";
            public const string Damaged = "DAMAGED";
            public const string Death = "DEATH";
            public const string Jump = "JUMP";
        }

        private readonly Animator _animator;
        private readonly Dictionary<PlayerStateType, string> _stateToAnimation;

        // 생성자 (Animator 참조 필요)
        public PlayerAnimator(Animator animator)
        {
            _animator = animator;
            _stateToAnimation = new Dictionary<PlayerStateType, string>
            {
                { PlayerStateType.Idle, AnimationNames.Idle },
                { PlayerStateType.Move, AnimationNames.Move },
                { PlayerStateType.Jump, AnimationNames.Jump },
                { PlayerStateType.Attack, AnimationNames.Attack },
                { PlayerStateType.Damaged, AnimationNames.Damaged },
                { PlayerStateType.Death, AnimationNames.Death }
            };
        }

        // 이벤트 구독 (Player의 OnEnable에서 호출)
        public void SubscribeEvents()
        {
            EventBus.Subscribe<PlayerStateChangedEvent>(OnPlayerStateChanged);
        }

        // 이벤트 구독 해제 (Player의 OnDisable에서 호출)
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<PlayerStateChangedEvent>(OnPlayerStateChanged);
        }

        // 상태 변경 이벤트 핸들러 - 애니메이션 재생
        private void OnPlayerStateChanged(PlayerStateChangedEvent evt)
        {
            PlayAnimation(evt.CurrentState);
        }

        // 상태에 따른 애니메이션 재생
        public void PlayAnimation(PlayerStateType stateType)
        {
            if (_stateToAnimation.TryGetValue(stateType, out string animationName))
            {
                _animator.Play(animationName);
            }
        }

        // 특정 애니메이션 직접 재생 (트리거 등 특수 상황용)
        public void PlayAnimation(string animationName)
        {
            _animator.Play(animationName);
        }

        // 현재 애니메이션 재생 완료 여부 확인
        public bool IsCurrentAnimationFinished()
        {
            if (_animator == null) return true;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1.0f;
        }
    }
}
