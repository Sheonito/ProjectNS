using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
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

        // 플레이어 데이터 (읽기 전용)
        private PlayerDataProvider _playerData;
        private bool _isPlayerDetected;
        private bool _wasPlayerDetected;
        private float _loseTargetTimer;

        // 공격 쿨타임
        private float _attackCooldownTimer;
        private float _attackCooldown;
        private float _attackRange;

        // Separation (외부에서 전달받음)
        private Vector2 _separationVelocity;

        // 생성자
        public EnemyMovement(Transform transform, EnemyMovementSettings settings)
        {
            _transform = transform;
            _settings = settings;
            
            // 기본값 설정 (StateSettings에서 덮어쓰기 가능)
            _attackCooldown = settings.attackCooldown;
            _attackRange = settings.attackRange;
        }

        // 공격 설정 (EnemyStateSettings에서 호출)
        public void SetAttackSettings(float attackCooldown, float attackRange)
        {
            _attackCooldown = attackCooldown;
            _attackRange = attackRange;
        }

        // 플레이어 데이터 설정 (Enemy에서 호출)
        public void SetPlayerData(PlayerDataProvider playerData)
        {
            _playerData = playerData;
        }

        // 물리 업데이트 (State에서 매 프레임 호출)
        public void UpdatePhysics()
        {
            CheckGround();
            HandleHorizontalMovement();
            UpdateSeparation();
            UpdateGravity();
            MoveCharacter();
            SnapToSlope();
            UpdateFacingDirection();
            UpdateAttackCooldown();
        }

        // 플레이어 탐지 업데이트 (State에서 호출)
        public void UpdateDetection()
        {
            _wasPlayerDetected = _isPlayerDetected;
            _isPlayerDetected = DetectPlayer();
        }

        // 카타나 제로 스타일 플레이어 탐지 (기본: 360도 거리 기반)
        private bool DetectPlayer()
        {
            if (_playerData == null) return false;

            Vector2 playerPos = _playerData.Position;
            Vector2 toPlayer = playerPos - (Vector2)_transform.position;
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
            if (_playerData == null) return;

            float direction = Mathf.Sign(_playerData.Position.x - _transform.position.x);
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

        // 지면 감지 + 땅 뚫기 방지
        private void CheckGround()
        {
            // 1. 땅 안에 묻혀있는지 확인 (위로 Raycast)
            Vector2 upOrigin = (Vector2)_transform.position;
            RaycastHit2D upHit = Physics2D.Raycast(upOrigin, Vector2.up, 2f, _settings.groundLayer);

            if (upHit.collider != null)
            {
                // 땅 안에 있음 → 즉시 땅 위로 이동
                // 충분히 위로 올린 후 아래로 Raycast해서 정확한 표면 찾기
                Vector2 aboveGround = upHit.point + Vector2.up * 0.1f;
                RaycastHit2D downHit = Physics2D.Raycast(aboveGround, Vector2.down, 3f, _settings.groundLayer);

                if (downHit.collider != null)
                {
                    float groundY = downHit.point.y + _settings.groundCheckOffset;
                    _transform.position = new Vector3(_transform.position.x, groundY, _transform.position.z);
                }
                else
                {
                    // fallback: 그냥 위로 밀어냄
                    _transform.position = new Vector3(_transform.position.x, upHit.point.y + 0.1f, _transform.position.z);
                }

                _velocity.y = 0;
                _isGrounded = true;
                return;
            }

            // 2. 정상적인 지면 감지 (아래로 Raycast)
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

        // 경사면 스냅 (앞쪽 아래로 Raycast하여 경사면 감지 및 Y 위치 조정)
        private void SnapToSlope()
        {
            // 지면에 있고 수평 이동 중일 때만
            if (!_isGrounded || Mathf.Abs(_velocity.x) < 0.01f) return;

            int moveDir = (int)Mathf.Sign(_velocity.x);

            // 앞쪽 아래 방향으로 Raycast (발 앞쪽 지점에서)
            Vector2 origin = (Vector2)_transform.position + Vector2.right * moveDir * 0.3f;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 1f, _settings.groundLayer);

            if (hit.collider == null) return;

            // 경사 각도 체크 (45도 이상이면 무시 - 벽으로 간주)
            float slopeAngle = Vector2.Angle(Vector2.up, hit.normal);
            if (slopeAngle > 45f) return;

            // 현재 발 위치와 경사면 높이 차이 계산
            float groundY = hit.point.y + _settings.groundCheckOffset;
            float diff = groundY - _transform.position.y;

            // 경사면이 위에 있으면 (올라가야 할 때) Y 위치 조정
            if (diff > 0.01f && diff < 0.5f)
            {
                _transform.position = new Vector3(_transform.position.x, groundY, _transform.position.z);
                _velocity.y = 0;
                _isGrounded = true;
            }
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
            if (_playerData == null) return false;

            float distance = Vector2.Distance(_transform.position, _playerData.Position);
            return distance <= _attackRange;
        }

        // 공격 가능 여부
        public bool CanAttack()
        {
            return _attackCooldownTimer <= 0 && IsInAttackRange();
        }

        // 공격 실행 (쿨타임 시작)
        public void StartAttackCooldown()
        {
            _attackCooldownTimer = _attackCooldown;
        }

        // 플레이어 방향 바라보기
        public void LookAtPlayer()
        {
            if (_playerData == null) return;

            float direction = Mathf.Sign(_playerData.Position.x - _transform.position.x);
            _facingDirection = (int)direction;
            Vector3 scale = _transform.localScale;
            scale.x = -_facingDirection;
            _transform.localScale = scale;
        }

        public bool IsPlayerDetected()
        {
            return _isPlayerDetected;
        }

        // 지면 체크 강제 실행 (스폰 시 호출)
        public void ForceGroundCheck()
        {
            CheckGround();
        }

        // 현재 위치 반환 (적 자신의 위치)
        public Vector2 GetPosition()
        {
            return _transform.position;
        }

        // 플레이어 위치 반환
        public Vector2 GetPlayerPosition()
        {
            if (_playerData == null)
                return Vector2.zero;
            return _playerData.Position;
        }

        // 플레이어가 땅에 있는지 확인
        public bool IsPlayerGrounded()
        {
            if (_playerData == null)
                return false;
            return _playerData.IsGrounded;
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
            Gizmos.DrawWireSphere(_transform.position, _attackRange);

            // 지면 체크
            Vector2 groundOrigin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * _settings.groundCheckDistance);
        }

#endif
    }
}
