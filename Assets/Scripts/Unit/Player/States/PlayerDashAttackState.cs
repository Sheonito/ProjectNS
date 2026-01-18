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
        private readonly PlayerAnimator _animator;
        private float _dashTimer;
        private float _dashDuration;
        private float _actualDashDistance;
        private int _dashDirection;
        private Vector3 _startPosition;
        private bool _hasHit;
        private bool _isDashing;
        private bool _isRecovering;
        private bool _isCooldownBlocked;
        private EnemyUnit _targetEnemy;

        // 쿨타임 관리 (static)
        private static float _lastDashTime = -999f;

        public PlayerDashAttackState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
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
            _targetEnemy = null;

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 쿨타임 체크
            if (IsOnCooldown())
            {
                _isCooldownBlocked = true;
                _isDashing = false;
                return;
            }

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            float totalDuration = _settings.dashAttackTargetDuration;
            _dashDuration = totalDuration * _settings.dashMoveRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.DashAttack);
            float autoSpeedFactor = baseAnimLength / totalDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            // 쿨타임 시작
            _lastDashTime = Time.time;

            _startPosition = _movement.GetPosition();

            // 대시 방향 결정 (마우스 방향)
            _dashDirection = GetMouseHorizontalDirection(_startPosition);
            _movement.SetFacingDirection(_dashDirection);

            // 장애물(벽+경사면) 체크: 거리를 고려해 실제 대시 거리 결정
            _actualDashDistance = _movement.GetObstacleDistance(_dashDirection, _settings.dashDistance);

            // 속도 초기화 (대시 중에는 별도로 위치 제어)
            _movement.SetVelocity(Vector2.zero);

            // 무적 상태 설정
            SetInvincible(true);

            // 대시 시작 시 타겟 미리 판정 (전체 대시 범위)
            FindTarget();

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
                float progress = _dashTimer / _dashDuration;

                if (progress < 1f)
                {
                    // X 위치 계산
                    float targetX = _startPosition.x + _dashDirection * _actualDashDistance;
                    float currentX = Mathf.Lerp(_startPosition.x, targetX, progress);

                    // 현재 X 위치에서 지면 Y 가져오기 (경사면 따라 이동)
                    float? groundY = _movement.GetGroundYAtPosition(currentX);
                    float currentY = groundY ?? _startPosition.y;

                    Vector3 currentPosition = new Vector3(currentX, currentY, _startPosition.z);
                    _movement.SetPosition(currentPosition);

                    // 대시 공격 데미지 적용 (타이밍에 1회만)
                    if (!_hasHit && progress >= _settings.dashAttackHitTimingRatio)
                    {
                        ApplyDamageToTarget();
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

        // 대시 시작 시 타겟 판정 (전체 대시 범위, 1명만)
        private void FindTarget()
        {
            Vector2 position = _startPosition;
            float dashDistance = _settings.dashDistance;

            // 대시 전체 범위에서 타겟 판정
            Vector2 attackCenter = position + Vector2.right * _dashDirection * dashDistance * 0.5f;
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackCenter,
                new Vector2(dashDistance, 1f),
                0f,
                _settings.enemyLayer
            );

            // 1명만 타겟으로 지정
            if (hits.Length > 0)
            {
                _targetEnemy = hits[0].GetComponent<EnemyUnit>();
            }
        }

        // 타겟에게 데미지 적용 (타이밍에 호출)
        private void ApplyDamageToTarget()
        {
            if (_targetEnemy != null)
            {
                _targetEnemy.OnDamaged(_settings.dashDamage);
            }
            _hasHit = true;
        }

        public override void Exit()
        {
            base.Exit();

            // 안전하게 무적 해제 (강제 상태 변경 대비)
            SetInvincible(false);
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
