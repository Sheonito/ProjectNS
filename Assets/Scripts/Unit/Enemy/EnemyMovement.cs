using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 적 이동 및 플레이어 탐지 처리 (카타나 제로 스타일)
    public class EnemyMovement
    {
        private readonly EnemyMovementSettings _settings;
        private readonly Transform _transform;

        private Vector2 _velocity;
        private float _horizontalInput;
        private bool _isGrounded;
        private int _facingDirection = 1;

        // 플레이어 탐지 관련
        private Transform _playerTransform;
        private bool _isPlayerDetected;
        private bool _wasPlayerDetected;
        private float _loseTargetTimer;

        // 공격 쿨타임
        private float _attackCooldownTimer;

        // Separation (외부에서 전달받음)
        private Vector2 _separationVelocity;

        // 생성자
        public EnemyMovement(Transform transform, EnemyMovementSettings settings)
        {
            _transform = transform;
            _settings = settings;
        }

        // 플레이어 Transform 설정 (Enemy에서 호출)
        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        // 물리 업데이트 (State에서 매 프레임 호출)
        public void UpdatePhysics()
        {
            CheckGround();
            HandleHorizontalMovement();
            UpdateSeparation();
            UpdateGravity();
            MoveCharacter();
            UpdateFacingDirection();
            UpdateAttackCooldown();
        }

        // 플레이어 탐지 업데이트 (State에서 호출)
        public void UpdateDetection()
        {
            _wasPlayerDetected = _isPlayerDetected;
            _isPlayerDetected = DetectPlayer();

            // 탐지 상태 변경 시 이벤트 발행
            if (_isPlayerDetected != _wasPlayerDetected)
            {
                EventBus.Publish(this, new EnemyPlayerDetectedEvent(_isPlayerDetected));
            }
        }

        // 카타나 제로 스타일 플레이어 탐지 (기본: 360도 거리 기반)
        private bool DetectPlayer()
        {
            if (_playerTransform == null) return false;

            Vector2 toPlayer = (Vector2)_playerTransform.position - (Vector2)_transform.position;
            float distance = toPlayer.magnitude;

            // 탐지 거리 초과
            if (distance > _settings.detectionRange)
            {
                return UpdateLoseTargetTimer();
            }

            // 시야각 사용 시에만 각도 체크
            if (_settings.useFieldOfView)
            {
                Vector2 facingDir = new Vector2(_facingDirection, 0);
                float angle = Vector2.Angle(facingDir, toPlayer.normalized);

                if (angle > _settings.detectionAngle)
                {
                    return UpdateLoseTargetTimer();
                }
            }

            // 장애물 체크 (Raycast) - obstacleLayer가 설정된 경우에만
            if (_settings.obstacleLayer != 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    _transform.position,
                    toPlayer.normalized,
                    distance,
                    _settings.obstacleLayer
                );

                if (hit.collider != null)
                {
                    return UpdateLoseTargetTimer();
                }
            }

            // 플레이어 발견 - 타이머 리셋
            _loseTargetTimer = _settings.loseTargetTime;
            return true;
        }

        // 타겟 상실 타이머 업데이트
        private bool UpdateLoseTargetTimer()
        {
            if (_loseTargetTimer > 0)
            {
                _loseTargetTimer -= Time.deltaTime;
                return _loseTargetTimer > 0;
            }

            return false;
        }

        // 이동 입력 설정
        public void SetHorizontalInput(float input)
        {
            _horizontalInput = input;
        }

        // 플레이어 방향으로 이동 입력 설정
        public void MoveTowardsPlayer()
        {
            if (_playerTransform == null) return;

            float direction = Mathf.Sign(_playerTransform.position.x - _transform.position.x);
            SetHorizontalInput(direction);
        }

        // 순찰 이동 (방향 설정)
        public void Patrol(int direction)
        {
            SetHorizontalInput(direction);
        }

        // 정지
        public void Stop()
        {
            SetHorizontalInput(0);
        }

        // 지면 감지
        private void CheckGround()
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _settings.groundCheckDistance,
                _settings.groundLayer);

            _isGrounded = hit.collider != null;

            if (_isGrounded && _velocity.y <= 0)
            {
                _velocity.y = 0;
            }
        }

        // 수평 이동 처리
        private void HandleHorizontalMovement()
        {
            float currentSpeed = _isPlayerDetected ? _settings.chaseSpeed : _settings.patrolSpeed;
            float targetSpeed = _horizontalInput * currentSpeed;
            float speedDiff = targetSpeed - _velocity.x;

            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _settings.acceleration : _settings.deceleration;
            float speedChange = Mathf.Sign(speedDiff) * accelRate * Time.deltaTime;

            if (Mathf.Abs(speedChange) >= Mathf.Abs(speedDiff))
            {
                _velocity.x = targetSpeed;
            }
            else
            {
                _velocity.x += speedChange;
            }

            // 벽 충돌 체크
            if (Mathf.Abs(_velocity.x) > 0.01f)
            {
                if (IsWallHit(Mathf.Sign(_velocity.x)))
                {
                    _velocity.x = 0;
                }
            }
        }

        // 적끼리 밀어내기 (StageManager에서 전달받은 velocity 적용)
        private void UpdateSeparation()
        {
            if (_separationVelocity.sqrMagnitude > 0.001f)
            {
                _velocity.x += _separationVelocity.x * Time.deltaTime;
                _separationVelocity = Vector2.zero;
            }
        }

        // Separation velocity 설정 (EnemyUnit에서 호출)
        public void SetSeparationVelocity(Vector2 velocity)
        {
            _separationVelocity = velocity;
        }

        // 중력 적용
        private void UpdateGravity()
        {
            if (!_isGrounded)
            {
                _velocity.y += _settings.gravity * Time.deltaTime;
                _velocity.y = Mathf.Max(_velocity.y, _settings.maxFallSpeed);
            }
        }

        // 이동 적용
        private void MoveCharacter()
        {
            Vector3 movement = new Vector3(_velocity.x, _velocity.y, 0) * Time.deltaTime;
            _transform.position += movement;
        }

        // 방향 전환
        private void UpdateFacingDirection()
        {
            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                _facingDirection = (int)Mathf.Sign(_horizontalInput);
                Vector3 scale = _transform.localScale;
                scale.x = -_facingDirection;
                _transform.localScale = scale;
            }
        }

        // 벽 감지
        private bool IsWallHit(float direction)
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.up * _settings.wallCheckHeight;
            Vector2 dir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, _settings.wallCheckDistance, _settings.groundLayer);
            return hit.collider != null;
        }

        // 공격 쿨타임 업데이트
        private void UpdateAttackCooldown()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        // 공격 범위 내 여부
        public bool IsInAttackRange()
        {
            if (_playerTransform == null) return false;

            float distance = Vector2.Distance(_transform.position, _playerTransform.position);
            return distance <= _settings.attackRange;
        }

        // 공격 가능 여부
        public bool CanAttack()
        {
            return _attackCooldownTimer <= 0 && IsInAttackRange();
        }

        // 공격 실행 (쿨타임 시작)
        public void StartAttackCooldown()
        {
            _attackCooldownTimer = _settings.attackCooldown;
        }

        // 플레이어 방향 바라보기
        public void LookAtPlayer()
        {
            if (_playerTransform == null) return;

            float direction = Mathf.Sign(_playerTransform.position.x - _transform.position.x);
            _facingDirection = (int)direction;
            Vector3 scale = _transform.localScale;
            scale.x = -_facingDirection;
            _transform.localScale = scale;
        }

        public bool IsPlayerDetected()
        {
            return _isPlayerDetected;
        }

#if UNITY_EDITOR

        // 디버그용 Gizmo 그리기
        public void DrawGizmos()
        {
            if (_transform == null) return;

            // 탐지 범위
            Gizmos.color = _isPlayerDetected ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(_transform.position, _settings.detectionRange);

            // 시야각 표시 (useFieldOfView가 true일 때만)
            if (_settings.useFieldOfView)
            {
                Vector3 facingDir = new Vector3(_facingDirection, 0, 0);
                Vector3 leftAngle = Quaternion.Euler(0, 0, _settings.detectionAngle) * facingDir;
                Vector3 rightAngle = Quaternion.Euler(0, 0, -_settings.detectionAngle) * facingDir;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_transform.position, _transform.position + leftAngle * _settings.detectionRange);
                Gizmos.DrawLine(_transform.position, _transform.position + rightAngle * _settings.detectionRange);
            }

            // 공격 범위
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_transform.position, _settings.attackRange);

            // 지면 체크
            Vector2 groundOrigin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * _settings.groundCheckDistance);
        }

#endif
    }
}