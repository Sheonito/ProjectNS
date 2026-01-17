using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    using Unit = Percent111.ProjectNS.Unit.Unit;

    public class PlayerUnit : Unit
    {
        [SerializeField] private PlayerMovementSettings _movementSettings;
        [SerializeField] private PlayerStateSettings _stateSettings;
        [SerializeField] private Animator _animator;

        private PlayerMovement _movement;
        private PlayerStateMachine _stateMachine;
        private PlayerAnimator _playerAnimator;
        private UIInputAction _inputAction;

        protected override void Awake()
        {
            base.Awake();

            _inputAction = UIInputAction.Instance;

            CreateMovement();
            CreateStateMachine();
            CreateAnimator();
        }

        private void OnEnable()
        {
            _inputAction.Player.Enable();
            SubscribeEvents();
            _stateMachine.SubscribeEvents();
            _playerAnimator.SubscribeEvents();
        }

        private void OnDisable()
        {
            _inputAction.Player.Disable();
            UnsubscribeEvents();
            _stateMachine.UnsubscribeEvents();
            _playerAnimator.UnsubscribeEvents();
        }

        // 이벤트 구독
        private void SubscribeEvents()
        {
            EventBus.Subscribe<PlayerStateChangedEvent>(OnPlayerStateChanged);
            EventBus.Subscribe<PlayerJumpEvent>(OnPlayerJump);
        }

        // 이벤트 구독 해제
        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<PlayerStateChangedEvent>(OnPlayerStateChanged);
            EventBus.Unsubscribe<PlayerJumpEvent>(OnPlayerJump);
        }

        // 상태 변경 이벤트 핸들러
        private void OnPlayerStateChanged(PlayerStateChangedEvent evt)
        {
            // 상태 변경 시 필요한 처리 (애니메이션, 사운드 등)
        }

        // 점프 이벤트 핸들러
        private void OnPlayerJump(PlayerJumpEvent evt)
        {
            // 점프 시 필요한 처리 (사운드, 이펙트 등)
        }

        // PlayerMovement 생성 및 설정
        private void CreateMovement()
        {
            _movement = new PlayerMovement(transform, _movementSettings);
        }

        // 상태 머신 초기화 (Movement를 State에 전달)
        private void CreateStateMachine()
        {
            _stateMachine = new PlayerStateMachine();

            PlayerIdleState idleState = new PlayerIdleState(_movement);
            PlayerMoveState moveState = new PlayerMoveState(_movement);
            PlayerJumpState jumpState = new PlayerJumpState(_movement);
            PlayerAttackState attackState = new PlayerAttackState(_movement, _stateSettings);
            PlayerDashAttackState dashAttackState = new PlayerDashAttackState(_movement, _stateSettings);

            _stateMachine.RegisterState(PlayerStateType.Idle, idleState);
            _stateMachine.RegisterState(PlayerStateType.Move, moveState);
            _stateMachine.RegisterState(PlayerStateType.Jump, jumpState);
            _stateMachine.RegisterState(PlayerStateType.Attack, attackState);
            _stateMachine.RegisterState(PlayerStateType.DashAttack, dashAttackState);

            _stateMachine.InitWithState(PlayerStateType.Idle);
        }

        // PlayerAnimator 생성
        private void CreateAnimator()
        {
            _playerAnimator = new PlayerAnimator(_animator);
        }

        private void Update()
        {
            _stateMachine.Execute();
        }

        // 데미지 처리 (이벤트 발행)
        public override void OnDamaged(int damage)
        {
            base.OnDamaged(damage);
            EventBus.Publish(this, new PlayerDamageEvent(damage, CurrentHp));
        }

        // 사망 처리 (이벤트 발행)
        protected override void OnDeath()
        {
            Debug.Log("Player Dead");
            EventBus.Publish(this, new PlayerDeathEvent());
        }

        // 디버그 시각화
        private void OnDrawGizmosSelected()
        {
            _movement?.DrawGizmos();
        }
    }
}
