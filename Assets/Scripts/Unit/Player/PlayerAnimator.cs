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
            public const string Idle = "Idle";
            public const string Run = "Run";
            public const string Attack = "Attack";
            public const string DashAttack = "Dash-Attack";
            public const string Dash = "Dash";
            public const string Hurt = "Hurt";
            public const string Death = "Death";
            public const string Jump = "JumpMerge";
            public const string Fall = "Fall";
            public const string Slide = "Slide";
            public const string Crouch = "Croush";
            public const string DiveAttack = "Dive-Attack";
        }

        private readonly Animator _animator;
        private readonly Dictionary<PlayerStateType, string> _stateToAnimation;
        private readonly Dictionary<string, float> _animationLengthCache;

        // 생성자 (Animator 참조 필요)
        public PlayerAnimator(Animator animator)
        {
            _animator = animator;
            _stateToAnimation = new Dictionary<PlayerStateType, string>
            {
                { PlayerStateType.Idle, AnimationNames.Idle },
                { PlayerStateType.Move, AnimationNames.Run },
                { PlayerStateType.Jump, AnimationNames.Jump },
                { PlayerStateType.Attack, AnimationNames.Attack },
                { PlayerStateType.JumpAttack, AnimationNames.Attack },
                { PlayerStateType.DashAttack, AnimationNames.DashAttack },
                { PlayerStateType.DiveAttack, AnimationNames.DiveAttack },
                { PlayerStateType.Backstep, AnimationNames.Dash },
                { PlayerStateType.Damaged, AnimationNames.Hurt },
                { PlayerStateType.Death, AnimationNames.Death }
            };
            _animationLengthCache = new Dictionary<string, float>();
            CacheAnimationLengths();
        }

        // 애니메이션 길이 캐싱 (최초 1회)
        private void CacheAnimationLengths()
        {
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
            foreach (AnimationClip clip in controller.animationClips)
            {
                _animationLengthCache[clip.name] = clip.length;
            }
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
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1.0f;
        }

        // 상태 타입에 해당하는 애니메이션 길이 반환
        public float GetAnimationLength(PlayerStateType stateType)
        {
            if (_stateToAnimation.TryGetValue(stateType, out string animationName))
            {
                return GetAnimationLength(animationName);
            }
            return 0f;
        }

        // 애니메이션 이름으로 길이 반환
        public float GetAnimationLength(string animationName)
        {
            if (_animationLengthCache.TryGetValue(animationName, out float length))
            {
                return length;
            }
            return 0f;
        }

        // 애니메이션 재생 속도 설정
        public void SetAnimationSpeed(float speed)
        {
            _animator.speed = speed;
        }

        // 애니메이션 재생 속도 초기화 (1.0)
        public void ResetAnimationSpeed()
        {
            _animator.speed = 1f;
        }
    }
}
