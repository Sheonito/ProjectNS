using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 유닛 클래스 (State와 EventBus로 통신, 직접 참조 없음)
    public class Player : Unit.Unit
    {
        [Header("이동 속도")]
        [SerializeField] private float _runSpeed = 12f;
        [SerializeField] private float _airSpeed = 10f;

        [Header("가속/감속 (카타나 제로 스타일: 빠른 가속, 즉각적인 정지)")]
        [SerializeField] private float _groundAcceleration = 80f;
        [SerializeField] private float _groundDeceleration = 100f;
        [SerializeField] private float _airAcceleration = 40f;
        [SerializeField] private float _airDeceleration = 30f;
        [SerializeField] private float _turnSpeedMultiplier = 2f;

        [Header("점프")]
        [SerializeField] private float _jumpForce = 18f;
        [SerializeField] private float _gravity = -50f;
        [SerializeField] private float _maxFallSpeed = -30f;
        [SerializeField] private float _coyoteTime = 0.08f;
        [SerializeField] private float _jumpCutMultiplier = 0.4f;

        [Header("지면 감지")]
        [SerializeField] private float _groundCheckDistance = 0.15f;
        [SerializeField] private float _groundCheckOffset = 0.5f;
        [SerializeField] private LayerMask _groundLayer = 1;

        [Header("벽 감지")]
        [SerializeField] private float _wallCheckDistance = 0.3f;
        [SerializeField] private float _wallCheckHeight = 0.3f;

        private PlayerMovement _movement;
        private PlayerStateMachine _stateMachine;
        private UIInputAction _inputAction;

        protected override void Awake()
        {
            base.Awake();
            
            _inputAction = UIInputAction.Instance;
            
            CreateMovement();
            CreateStateMachine();
        }

        private void OnEnable()
        {
            _inputAction.Player.Enable();
            SubscribeEvents();
            _stateMachine.SubscribeEvents();
        }

        private void OnDisable()
        {
            _inputAction.Player.Disable();
            UnsubscribeEvents();
            _stateMachine.UnsubscribeEvents();
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
            Debug.Log($"Player State: {evt.PreviousState} → {evt.CurrentState}");
        }

        // 점프 이벤트 핸들러
        private void OnPlayerJump(PlayerJumpEvent evt)
        {
            // 점프 시 필요한 처리 (사운드, 이펙트 등)
        }

        // PlayerMovement 생성 및 설정
        private void CreateMovement()
        {
            _movement = new PlayerMovement(transform, _groundLayer);
            _movement.SetMovementSettings(_runSpeed, _airSpeed, _groundAcceleration, _groundDeceleration, _airAcceleration, _airDeceleration, _turnSpeedMultiplier);
            _movement.SetJumpSettings(_jumpForce, _gravity, _maxFallSpeed, _coyoteTime, _jumpCutMultiplier);
            _movement.SetGroundCheckSettings(_groundCheckDistance, _groundCheckOffset);
            _movement.SetWallCheckSettings(_wallCheckDistance, _wallCheckHeight);
        }

        // 상태 머신 초기화 (Movement를 State에 전달)
        private void CreateStateMachine()
        {
            _stateMachine = new PlayerStateMachine();

            PlayerIdleState idleState = new PlayerIdleState(_movement);
            PlayerMoveState moveState = new PlayerMoveState(_movement);
            PlayerJumpState jumpState = new PlayerJumpState(_movement);

            _stateMachine.RegisterState(PlayerStateType.Idle, idleState);
            _stateMachine.RegisterState(PlayerStateType.Move, moveState);
            _stateMachine.RegisterState(PlayerStateType.Jump, jumpState);

            _stateMachine.InitWithState(PlayerStateType.Idle);
        }

        private void Update()
        {
            _stateMachine.Execute();
        }

        // 데미지 처리 (이벤트 발행)
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
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
