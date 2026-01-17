using DG.Tweening;
using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    using Unit = Percent111.ProjectNS.Unit.Unit;

    // 적 기본 클래스
    public class EnemyUnit : Unit
    {
        [SerializeField] private EnemyMovementSettings _movementSettings;
        [SerializeField] private EnemyStateSettings _stateSettings;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteGroup _spriteGroup;
        [SerializeField] private float _fadeDuration = 0.5f;

        private EnemyMovement _movement;
        private Transform _playerTransform;
        private EnemyStateMachine _stateMachine;
        private EnemyAnimator _enemyAnimator;

        protected override void Awake()
        {
            base.Awake();

            CreateMovement();
            CreateStateMachine();
            CreateAnimator();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            _stateMachine.SubscribeEvents();
            _enemyAnimator?.SubscribeEvents();
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
            EventBus.Subscribe<EnemyStateChangedEvent>(OnEnemyStateChanged);
            EventBus.Subscribe<EnemyAttackEvent>(OnEnemyAttack);
            EventBus.Subscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        // 이벤트 구독 해제
        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<EnemyStateChangedEvent>(OnEnemyStateChanged);
            EventBus.Unsubscribe<EnemyAttackEvent>(OnEnemyAttack);
            EventBus.Unsubscribe<EnemyDeathCompleteEvent>(OnEnemyDeathComplete);
        }

        // 상태 변경 이벤트 핸들러
        private void OnEnemyStateChanged(EnemyStateChangedEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _movement)
                return;

            // 상태 변경 시 필요한 처리 (사운드, 이펙트 등)
        }

        // 공격 이벤트 핸들러
        private void OnEnemyAttack(EnemyAttackEvent evt)
        {
            // 자신의 이벤트만 처리
            if (evt.Owner != _movement)
                return;

            // 공격 판정 처리 (플레이어에게 데미지)
            if (_movement.IsInAttackRange())
            {
                PlayerUnit player = _playerTransform?.GetComponent<PlayerUnit>();
                player?.OnDamaged(evt.Damage);
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

        // Pool에서 재사용 시 초기화
        public void ResetForPool()
        {
            // HP 복구
            _currentHp = MaxHp;

            // 색상 복구
            RestoreSpriteColors();

            // 상태 초기화
            _stateMachine.InitWithState(EnemyStateType.Idle);
        }

        // Movement 생성
        private void CreateMovement()
        {
            _movement = new EnemyMovement(transform, _movementSettings);
        }

        // 상태 머신 초기화
        private void CreateStateMachine()
        {
            _stateMachine = new EnemyStateMachine(_movement);

            EnemyIdleState idleState = new EnemyIdleState(_movement, _stateSettings);
            EnemyPatrolState patrolState = new EnemyPatrolState(_movement, _stateSettings);
            EnemyChaseState chaseState = new EnemyChaseState(_movement);
            EnemyAttackState attackState = new EnemyAttackState(_movement, _stateSettings);
            EnemyDamagedState damagedState = new EnemyDamagedState(_movement, _stateSettings);
            EnemyDeathState deathState = new EnemyDeathState(_movement, _stateSettings);

            _stateMachine.RegisterState(EnemyStateType.Idle, idleState);
            _stateMachine.RegisterState(EnemyStateType.Patrol, patrolState);
            _stateMachine.RegisterState(EnemyStateType.Chase, chaseState);
            _stateMachine.RegisterState(EnemyStateType.Attack, attackState);
            _stateMachine.RegisterState(EnemyStateType.Damaged, damagedState);
            _stateMachine.RegisterState(EnemyStateType.Death, deathState);

            _stateMachine.InitWithState(EnemyStateType.Idle);
        }

        // Animator 생성
        private void CreateAnimator()
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

        // 데미지 처리 (이벤트 발행)
        public override void OnDamaged(int damage)
        {
            if (IsDead)
                return;

            base.OnDamaged(damage);
            EventBus.Publish(this, new EnemyDamageEvent(damage, CurrentHp));

            // 피격 상태로 전환
            if (!IsDead)
            {
                EventBus.Publish(this, new EnemyForceStateChangeEvent(EnemyStateType.Damaged, _movement));
            }
        }

        // 사망 처리
        protected override void OnDeath()
        {
            Debug.Log($"Enemy Dead: {gameObject.name}");
            EventBus.Publish(this, new EnemyDeathEvent());
            EventBus.Publish(this, new EnemyForceStateChangeEvent(EnemyStateType.Death, _movement));
        }

        // 플레이어 Transform 설정 (외부에서 호출)
        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
            _movement.SetPlayerTransform(playerTransform);
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
