using System;
using DG.Tweening;
using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    using Unit = Percent111.ProjectNS.Unit.Unit;

    // 적 기본 추상 클래스
    public abstract class EnemyUnit : Unit
    {
        [SerializeField] protected EnemyMovementSettings _movementSettings;
        [SerializeField] protected EnemyStateSettings _stateSettings;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected SpriteGroup _spriteGroup;
        [SerializeField] protected float _fadeDuration = 0.5f;

        protected EnemyMovement _movement;
        protected PlayerDataProvider _playerData;
        protected EnemyStateMachine _stateMachine;
        protected EnemyAnimator _enemyAnimator;

        protected override void Awake()
        {
            base.Awake();

            CreateMovement();
            CreateAnimator();
            CreateStateMachine();
        }
        
        private void OnEnable()
        {
            SubscribeEvents();
            _stateMachine.SubscribeEvents();
            _enemyAnimator?.SubscribeEvents();

            // 활성화 시 지면 체크 (땅 뚫기 방지)
            _movement?.ForceGroundCheck();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            _stateMachine.UnsubscribeEvents();
            _enemyAnimator?.UnsubscribeEvents();
        }

        // 이벤트 구독
        private void SubscribeEvents()
        {
            EventBus.Subscribe<EnemyAttackEvent>(OnEnemyAttack);
            EventBus.Subscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        // 이벤트 구독 해제
        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyAttackEvent>(OnEnemyAttack);
            EventBus.Unsubscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        // 공격 이벤트 핸들러 (하위 클래스에서 오버라이드 가능)
        protected virtual void OnEnemyAttack(EnemyAttackEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _movement)
                return;

            // 기본 근거리 공격 판정 처리 (플레이어에게 데미지)
            if (_movement.IsInAttackRange())
            {
                _playerData?.ApplyDamage(evt.Damage);
            }
        }

        // 사망 완료 이벤트 핸들러
        private void OnEnemyDeathComplete(EnemyDeathCompleteEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _movement)
                return;

            // Fade Out 시작 (DOTween)
            FadeOutAndReturnToPool();
        }

        // Fade Out 후 Pool 반환 (DOTween)
        private async void FadeOutAndReturnToPool()
        {
            await _spriteGroup.DOFade(0, _fadeDuration);
            OnFadeComplete();
        }

        // Fade 완료 콜백
        private void OnFadeComplete()
        {
            // 색상 복구 (Pool에서 재사용 시 필요)
            RestoreSpriteColors();

            // Pool 반환 이벤트 발행
            EventBus.Publish(this, new EnemyReturnToPoolEvent(this));
        }

        // Sprite 색상 복구
        private void RestoreSpriteColors()
        {
            _spriteGroup.DOFade(1, _fadeDuration);
        }

        // Pool에서 재사용 시 초기화 (하위 클래스에서 오버라이드 가능)
        public virtual void ResetForPool()
        {
            // HP 복구
            _currentHp = MaxHp;

            // 색상 복구
            RestoreSpriteColors();

            // 상태 초기화
            _stateMachine.InitWithState(EnemyStateType.Idle);

            // 지면 체크 강제 실행 (땅 뚫기 방지)
            _movement.ForceGroundCheck();
        }

        // Movement 생성
        protected virtual void CreateMovement()
        {
            _movement = new EnemyMovement(transform, _movementSettings);
        }

        // 상태 머신 초기화 (하위 클래스에서 오버라이드 가능)
        protected virtual void CreateStateMachine()
        {
            _stateMachine = new EnemyStateMachine(_movement);

            EnemyIdleState idleState = new EnemyIdleState(_movement, _stateSettings);
            EnemyPatrolState patrolState = new EnemyPatrolState(_movement, _stateSettings);
            EnemyChaseState chaseState = new EnemyChaseState(_movement);
            EnemyAttackState attackState = new EnemyAttackState(_movement, _stateSettings, _enemyAnimator);
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

        // Animator 생성
        protected virtual void CreateAnimator()
        {
            if (_animator != null)
            {
                _enemyAnimator = new EnemyAnimator(_animator, _movement);
            }
        }

        private void Update()
        {
            if (IsDead)
                return;

            _stateMachine.Execute();
        }

        // 데미지 처리 (직접 상태 전환)
        public void OnDamaged(int damage)
        {
            if (IsDead)
                return;

            ApplyDamage(damage);

            // 타격 연출 (카메라 쉐이크 + 히트 이펙트)
            HitEffectManager.Instance?.PlayHitEffect(transform.position);

            // HP에 따라 상태 전환
            if (IsDead)
            {
                Debug.Log($"Enemy Dead: {gameObject.name}");
                _stateMachine.ChangeState(EnemyStateType.Death);
            }
            else
            {
                _stateMachine.ChangeState(EnemyStateType.Damaged);
            }
        }

        // 플레이어 데이터 설정 (외부에서 호출)
        public void SetPlayerData(PlayerDataProvider playerData)
        {
            _playerData = playerData;
            _movement.SetPlayerData(playerData);
        }

        // Separation 힘 업데이트 (StageManager에서 호출)
        public void UpdateSeparationForce(Vector2 force)
        {
            _movement.SetSeparationVelocity(force);
        }

#if UNITY_EDITOR
        // 디버그 시각화
        private void OnDrawGizmosSelected()
        {
            _movement.DrawGizmos();
        }
#endif
    }
}
