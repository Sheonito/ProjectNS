using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 공격 상태 (슬래시 공격 - 살짝 대시 포함)
    public class PlayerAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _attackTimer;
        private float _attackDuration;
        private float _slashDuration;
        private float _actualSlashDistance;
        private float _hitTiming;
        private bool _hasHit;
        private int _attackDirection;
        private Vector3 _startPosition;
        private bool _isSlashing;

        public PlayerAttackState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
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
            _startPosition = _movement.GetPosition();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _attackDuration = _settings.attackTargetDuration;
            _slashDuration = _attackDuration * _settings.slashDashRatio;
            _hitTiming = _attackDuration * _settings.attackHitTimingRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.Attack);
            float autoSpeedFactor = baseAnimLength / _attackDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 마우스 방향에 따라 플레이어 방향 설정
            Vector2 playerPos = _movement.GetPosition();
            _attackDirection = GetMouseHorizontalDirection(playerPos);
            _movement.SetFacingDirection(_attackDirection);

            // 적이 공격 범위 내에 있으면 제자리 공격, 없으면 슬래시 대시
            bool hasEnemyInRange = IsEnemyInAttackRange(playerPos, _settings.attackRange, _settings.enemyLayer);
            _isSlashing = !hasEnemyInRange && _movement.IsGrounded();

            // 슬래시 대시 시 장애물(벽+경사면) 체크
            if (_isSlashing)
            {
                _actualSlashDistance = _movement.GetObstacleDistance(_attackDirection, _settings.slashDashDistance);
            }

            // 공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerAttackEvent());
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;

            // 슬래시 대시 중 (살짝 앞으로 이동)
            if (_isSlashing && _attackTimer < _slashDuration)
            {
                float progress = _attackTimer / _slashDuration;

                // X 위치 계산
                float targetX = _startPosition.x + _attackDirection * _actualSlashDistance;
                float currentX = Mathf.Lerp(_startPosition.x, targetX, progress);

                // 현재 X 위치에서 지면 Y 가져오기 (경사면 따라 이동)
                float? groundY = _movement.GetGroundYAtPosition(currentX);
                float currentY = groundY ?? _startPosition.y;

                Vector3 currentPosition = new Vector3(currentX, currentY, _startPosition.z);
                _movement.SetPosition(currentPosition);
            }
            else
            {
                _isSlashing = false;
                // 슬래시 완료 후 물리 업데이트
                _movement.UpdatePhysics();
            }

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

            // 공격 완료
            if (_attackTimer >= _attackDuration)
            {
                float horizontalInput = GetHorizontalInput();

                // 입력에 따라 상태 전환
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
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
