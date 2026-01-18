using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 점프 찍기 공격 상태 (공중에서 대각선 아래로 내려찍는 공격)
    public class PlayerDiveAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _diveTimer;
        private float _diveDuration;
        private float _recoveryTime;
        private int _diveDirection;
        private bool _isDiving;
        private bool _isRecovering;
        private bool _hasHit;

        public PlayerDiveAttackState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _diveTimer = 0;
            _hasHit = false;
            _isDiving = true;
            _isRecovering = false;

            // 수평 입력 초기화
            _movement.SetHorizontalInput(0);

            // 목표 duration 기반 계산
            _diveDuration = _settings.diveAttackTargetDuration;
            _recoveryTime = _settings.diveAttackRecoveryTime;

            // 마우스 방향에 따라 공격 방향 설정
            Vector2 playerPos = _movement.GetPosition();
            _diveDirection = GetMouseHorizontalDirection(playerPos);
            _movement.SetFacingDirection(_diveDirection);

            // 대각선 아래로 찍기 위한 회전 (포크 느낌)
            float rotationAngle = -45f * _diveDirection;
            _movement.SetRotation(rotationAngle);

            // 하강 속도 설정 (대각선 아래 방향)
            Vector2 diveVelocity = new Vector2(
                _diveDirection * _settings.diveAttackHorizontalSpeed,
                -_settings.diveAttackSpeed
            );
            _movement.SetVelocity(diveVelocity);

            // 애니메이션 속도 자동 계산
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.DiveAttack);
            if (baseAnimLength > 0 && _diveDuration > 0)
            {
                float autoSpeedFactor = baseAnimLength / _diveDuration;
                _animator.SetAnimationSpeed(autoSpeedFactor);
            }

            // 이벤트 발행 (사운드/이펙트용)
            EventBus.Publish(this, new PlayerDiveAttackEvent(_diveDirection));
        }

        public override void Execute()
        {
            base.Execute();

            _diveTimer += Time.deltaTime;

            // 찍기 중
            if (_isDiving)
            {
                Vector2 velocity = _movement.GetVelocity();
                float moveDistance = Mathf.Abs(velocity.y) * Time.deltaTime;

                // 지면까지 거리 체크
                float groundDistance = _movement.GetGroundDistance(moveDistance + 0.5f);

                // 지면에 닿으면 착지
                if (groundDistance <= moveDistance)
                {
                    // 지면 위치로 스냅
                    Vector3 pos = _movement.GetPosition();
                    pos.y -= groundDistance;
                    _movement.SetPosition(pos);
                    _movement.ForceGroundCheck();
                    OnLanded();
                    return;
                }

                // 벽 충돌 체크
                float wallDistance = _movement.GetWallDistance(_diveDirection, 0.5f);
                if (wallDistance < 0.1f)
                {
                    // 벽에 닿으면 수평 이동 중지
                    _movement.SetVelocity(new Vector2(0, velocity.y));
                    velocity = _movement.GetVelocity();
                }

                // 내려찍기 중 공격 판정 (1회만)
                if (!_hasHit)
                {
                    PerformDiveHit();
                }

                // 위치 이동 (지면 체크 후 안전하게 이동)
                Vector3 movement = new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
                _movement.SetPosition(_movement.GetPosition() + movement);
            }
            // 착지 후 경직 중
            else if (_isRecovering)
            {
                if (_diveTimer >= _recoveryTime)
                {
                    float horizontalInput = GetHorizontalInput();

                    if (Mathf.Abs(horizontalInput) > 0.01f)
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
        }

        // 착지 시 처리
        private void OnLanded()
        {
            _isDiving = false;
            _isRecovering = true;
            _diveTimer = 0;

            // 속도 초기화
            _movement.SetVelocity(Vector2.zero);

            // 회전 복원
            _movement.ResetRotation();

            // 착지 시 공격 판정 (아직 안 맞췄으면)
            if (!_hasHit)
            {
                PerformDiveHit();
            }
        }

        // 찍기 공격 판정 처리 (1명만 타격)
        private void PerformDiveHit()
        {
            Vector2 position = _movement.GetPosition();
            float range = _settings.diveAttackRange;

            // 대각선 아래 방향으로 판정
            Vector2 attackCenter = position + new Vector2(_diveDirection * range * 0.3f, -range * 0.5f);
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

            if (closestEnemy != null)
            {
                closestEnemy.OnDamaged(_settings.diveAttackDamage);
                _hasHit = true;
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 회전 복원
            _movement.ResetRotation();

            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
