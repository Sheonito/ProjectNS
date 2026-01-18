using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 점프 공격 상태 (대각선 점프 + 공격, 회전 포함)
    public class PlayerJumpAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _attackTimer;
        private float _attackDuration;
        private float _hitTiming;
        private bool _hasHit;
        private bool _hasJumped;
        private int _jumpDirection;

        // 대각선 공격 회전 각도 (오른쪽으로 이동 시 -45도, 왼쪽은 +45도)
        private const float DIAGONAL_ROTATION_ANGLE = 45f;

        public PlayerJumpAttackState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasHit = false;
            _hasJumped = false;

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _attackDuration = _settings.jumpAttackTargetDuration;
            _hitTiming = _attackDuration * _settings.attackHitTimingRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.JumpAttack);
            float autoSpeedFactor = baseAnimLength / _attackDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 마우스 방향에 따라 플레이어 방향 설정
            Vector2 playerPos = _movement.GetPosition();
            _jumpDirection = GetMouseHorizontalDirection(playerPos);
            _movement.SetFacingDirection(_jumpDirection);

            // 대각선 회전 적용 (이동 방향에 따라 기울임)
            float rotationAngle = -_jumpDirection * DIAGONAL_ROTATION_ANGLE;
            _movement.SetRotation(rotationAngle);

            // 대각선 점프 실행 (점프력 약간 낮춤 + 전방 이동 속도 설정)
            if (_movement.CanJump())
            {
                _movement.DiagonalJump(_settings.jumpAttackJumpMultiplier, _jumpDirection * _settings.jumpAttackForwardSpeed);
                _hasJumped = true;
                PublishJumpEvent();
            }

            // 점프 공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerJumpAttackEvent(_jumpDirection));
        }

        public override void Execute()
        {
            base.Execute();

            // 대각선 점프 중에는 입력 무시, 물리만 업데이트
            _movement.UpdatePhysics();

            _attackTimer += Time.deltaTime;

            // 공격 판정 타이밍
            if (!_hasHit && _attackTimer >= _hitTiming)
            {
                _hasHit = true;
                PerformAttackHit();
            }

            // 대시공격 입력 시 대시공격으로 전환
            if (IsDashAttackPressed())
            {
                RequestStateChange(PlayerStateType.DashAttack);
                return;
            }

            // 공격 완료 후 착지 여부에 따라 상태 전환
            if (_attackTimer >= _attackDuration)
            {
                float horizontalInput = GetHorizontalInput();

                if (!_movement.IsGrounded())
                {
                    RequestStateChange(PlayerStateType.Jump);
                }
                else if (Mathf.Abs(horizontalInput) > 0.01f)
                {
                    RequestStateChange(PlayerStateType.Move);
                }
                else
                {
                    RequestStateChange(PlayerStateType.Idle);
                }
                return;
            }
        }

        // 공격 판정 처리 (State에서 직접 처리, 1명만 타격)
        private void PerformAttackHit()
        {
            Vector2 position = _movement.GetPosition();
            Vector2 attackDirection = GetMouseDirection(position);
            float range = _settings.attackRange;

            Vector2 attackCenter = position + attackDirection * range * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, range, _settings.enemyLayer);

            // 1명만 타격 (가장 가까운 적)
            float closestDistance = float.MaxValue;
            EnemyUnit closestEnemy = null;

            foreach (Collider2D hit in hits)
            {
                EnemyUnit enemy = hit.GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    float distance = Vector2.Distance(position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }

            closestEnemy?.OnDamaged(_settings.attackDamage);
        }

        public override void Exit()
        {
            base.Exit();

            // 회전 초기화
            _movement.ResetRotation();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
