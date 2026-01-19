using Cysharp.Threading.Tasks;
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

        [Header("Shield")]
        [SerializeField] private GameObject _shieldVisual; // 방어막 시각 효과 (선택)

        private PlayerMovement _movement;
        private PlayerStateMachine _stateMachine;
        private PlayerAnimator _playerAnimator;
        private UIInputAction _inputAction;
        private bool _isInvincible;
        private bool _hasShield; // 방어막 보유 여부

        // 무적 상태 여부 (투사체 통과 판정용)
        public bool IsInvincible => _isInvincible;
        public bool HasShield => _hasShield;

        protected override void Awake()
        {
            base.Awake();

            _inputAction = UIInputAction.Instance;

            CreateMovement();
            CreateAnimator();
            CreateStateMachine();
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
            EventBus.Subscribe<PlayerInvincibleEvent>(OnPlayerInvincible);
            EventBus.Subscribe<ShieldCollectedEvent>(OnShieldCollected);
        }

        // 이벤트 구독 해제
        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<PlayerInvincibleEvent>(OnPlayerInvincible);
            EventBus.Unsubscribe<ShieldCollectedEvent>(OnShieldCollected);
        }

        // 무적 상태 변경 이벤트 핸들러
        private void OnPlayerInvincible(PlayerInvincibleEvent evt)
        {
            _isInvincible = evt.IsInvincible;
        }

        // 방어막 획득 이벤트 핸들러
        private void OnShieldCollected(ShieldCollectedEvent evt)
        {
            _hasShield = true;
            if (_shieldVisual != null)
            {
                _shieldVisual.SetActive(true);
            }
        }

        // 방어막 파괴 처리
        private void BreakShield()
        {
            _hasShield = false;
            if (_shieldVisual != null)
            {
                _shieldVisual.SetActive(false);
            }
            
            // 무적 시간 부여
            StartShieldInvincibility().Forget();
        }

        // 방어막 파괴 후 무적 시간
        private async UniTaskVoid StartShieldInvincibility()
        {
            _isInvincible = true;
            await UniTask.WaitForSeconds(_stateSettings.shield.invincibleDuration);
            _isInvincible = false;
        }

        // PlayerMovement 생성 및 설정
        private void CreateMovement()
        {
            _movement = new PlayerMovement(transform, _movementSettings);
        }

        // 상태 머신 초기화 (Movement와 Animator를 State에 전달)
        private void CreateStateMachine()
        {
            _stateMachine = new PlayerStateMachine();

            PlayerIdleState idleState = new PlayerIdleState(_movement, _stateSettings);
            PlayerMoveState moveState = new PlayerMoveState(_movement, _stateSettings);
            PlayerJumpState jumpState = new PlayerJumpState(_movement, _stateSettings);
            PlayerAttackState attackState = new PlayerAttackState(_movement, _stateSettings, _playerAnimator);
            PlayerJumpAttackState jumpAttackState = new PlayerJumpAttackState(_movement, _stateSettings, _playerAnimator);
            PlayerDashAttackState dashAttackState = new PlayerDashAttackState(_movement, _stateSettings, _playerAnimator);
            PlayerDiveAttackState diveAttackState = new PlayerDiveAttackState(_movement, _stateSettings, _playerAnimator);
            PlayerBackstepState backstepState = new PlayerBackstepState(_movement, _stateSettings, _playerAnimator);
            PlayerDamagedState damagedState = new PlayerDamagedState(_movement, _stateSettings, _playerAnimator);
            PlayerDeathState deathState = new PlayerDeathState(_movement, _stateSettings, _playerAnimator);

            _stateMachine.RegisterState(PlayerStateType.Idle, idleState);
            _stateMachine.RegisterState(PlayerStateType.Move, moveState);
            _stateMachine.RegisterState(PlayerStateType.Jump, jumpState);
            _stateMachine.RegisterState(PlayerStateType.Attack, attackState);
            _stateMachine.RegisterState(PlayerStateType.JumpAttack, jumpAttackState);
            _stateMachine.RegisterState(PlayerStateType.DashAttack, dashAttackState);
            _stateMachine.RegisterState(PlayerStateType.DiveAttack, diveAttackState);
            _stateMachine.RegisterState(PlayerStateType.Backstep, backstepState);
            _stateMachine.RegisterState(PlayerStateType.Damaged, damagedState);
            _stateMachine.RegisterState(PlayerStateType.Death, deathState);

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

        private void FixedUpdate()
        {
            _stateMachine.ExecutePhysics();
        }

        // 데미지 처리 (직접 상태 전환)
        public void OnDamaged(int damage)
        {
            // 무적 상태면 데미지 무시
            if (_isInvincible)
            {
                return;
            }

            // 이미 사망 상태면 무시
            if (_stateMachine.GetCurrentStateType() == PlayerStateType.Death)
            {
                return;
            }

            // 방어막이 있으면 방어막 파괴 (데미지 무효)
            if (_hasShield)
            {
                BreakShield();
                return;
            }

            ApplyDamage(damage);

            // HP에 따라 상태 전환
            if (IsDead)
            {
                _stateMachine.ChangeState(PlayerStateType.Death);
            }
            else
            {
                _stateMachine.ChangeState(PlayerStateType.Damaged);
            }
        }

        // PlayerDataProvider 생성 (외부에서 호출)
        public PlayerDataProvider CreateDataProvider()
        {
            return new PlayerDataProvider(_movement, OnDamaged);
        }

        // 디버그 시각화
        private void OnDrawGizmosSelected()
        {
            _movement?.DrawGizmos();
            DrawAttackRangeGizmos();
        }

        // 공격 범위 기즈모
        private void DrawAttackRangeGizmos()
        {
            if (_stateSettings == null) return;

            Vector3 position = transform.position;

            // 일반 공격 범위 (노란색)
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(position, _stateSettings.attack.range);

            // 다이브 공격 범위 (주황색)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(position, _stateSettings.diveAttack.range);
        }
    }
}
