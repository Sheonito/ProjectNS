using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 대시베기 상태 (쿨타임 적용)
    public class PlayerDashAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private float _dashTimer;
        private int _dashDirection;
        private Vector3 _startPosition;
        private bool _hasHit;
        private bool _isDashing;
        private bool _isRecovering;
        private bool _isCooldownBlocked;

        // 쿨타임 관리 (static)
        private static float _lastDashTime = -999f;

        public PlayerDashAttackState(PlayerMovement movement, PlayerStateSettings settings) : base()
        {
            _movement = movement;
            _settings = settings;
        }

        // 쿨타임 체크 (외부에서 호출 가능)
        public bool IsOnCooldown()
        {
            return Time.time - _lastDashTime < _settings.dashCooldown;
        }

        public override void Enter()
        {
            base.Enter();
            _dashTimer = 0;
            _hasHit = false;
            _isDashing = true;
            _isRecovering = false;
            _isCooldownBlocked = false;

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 쿨타임 체크
            if (IsOnCooldown())
            {
                _isCooldownBlocked = true;
                _isDashing = false;
                return;
            }

            // 쿨타임 시작
            _lastDashTime = Time.time;

            _startPosition = _movement.GetPosition();

            // 대시 방향 결정 (마우스 방향)
            _dashDirection = GetMouseHorizontalDirection(_startPosition);
            _movement.SetFacingDirection(_dashDirection);

            // 속도 초기화 (대시 중에는 별도로 위치 제어)
            _movement.SetVelocity(Vector2.zero);

            // 무적 상태 설정
            SetInvincible(true);

            // 대시공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerDashAttackEvent(_dashDirection));
        }

        public override void Execute()
        {
            base.Execute();

            // 쿨타임으로 차단된 경우 바로 상태 전환
            if (_isCooldownBlocked)
            {
                if (!_movement.IsGrounded())
                {
                    RequestStateChange(PlayerStateType.Jump);
                }
                else
                {
                    RequestStateChange(PlayerStateType.Idle);
                }
                return;
            }

            _dashTimer += Time.deltaTime;

            // 대시 중
            if (_isDashing)
            {
                float progress = _dashTimer / _settings.dashDuration;

                if (progress < 1f)
                {
                    Vector3 targetPosition = _startPosition + Vector3.right * _dashDirection * _settings.dashDistance;
                    Vector3 currentPosition = Vector3.Lerp(_startPosition, targetPosition, progress);
                    _movement.SetPosition(currentPosition);

                    // 대시 중 공격 판정 (1회만, 1명만)
                    if (!_hasHit)
                    {
                        PerformDashAttackHit();
                    }
                }
                else
                {
                    // 대시 완료 → 후딜레이 시작
                    _isDashing = false;
                    _isRecovering = true;
                    _dashTimer = 0;

                    // 무적 해제
                    SetInvincible(false);
                }
                return;
            }

            // 후딜레이 중
            if (_isRecovering)
            {
                _movement.UpdatePhysics();

                if (_dashTimer >= _settings.dashRecoveryTime)
                {
                    // 후딜레이 완료 → 상태 전환
                    if (!_movement.IsGrounded())
                    {
                        RequestStateChange(PlayerStateType.Jump);
                    }
                    else
                    {
                        RequestStateChange(PlayerStateType.Idle);
                    }
                }
            }
        }

        // 대시 공격 판정 처리 (State에서 직접 처리, 1명 제한)
        private void PerformDashAttackHit()
        {
            Vector2 position = _movement.GetPosition();
            float dashDistance = _settings.dashDistance;

            Vector2 attackCenter = position + Vector2.right * _dashDirection * dashDistance * 0.5f;
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackCenter,
                new Vector2(dashDistance, 1f),
                0f,
                _settings.enemyLayer
            );

            // 1명만 타격
            if (hits.Length > 0)
            {
                EnemyUnit enemy = hits[0].GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    enemy.OnDamaged(_settings.dashDamage);
                    _hasHit = true;
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 안전하게 무적 해제 (강제 상태 변경 대비)
            SetInvincible(false);
        }
    }
}
