using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 이동 물리 처리 (일반 C# 클래스, State에서 호출하여 사용)
    public class PlayerMovement
    {
        private readonly PlayerMovementSettings _settings;
        private readonly Transform _transform;

        private Vector2 _velocity;
        private float _horizontalInput;
        private bool _isGrounded;
        private bool _wasGrounded;
        private float _coyoteTimer;
        private int _facingDirection = 1;
        private bool _jumpCutApplied;

        // 생성자
        public PlayerMovement(Transform transform, PlayerMovementSettings settings)
        {
            _transform = transform;
            _settings = settings ?? new PlayerMovementSettings();
        }

        // 입력 설정
        public void SetHorizontalInput(float input)
        {
            _horizontalInput = input;
        }

        // 물리 업데이트 (State에서 매 프레임 호출)
        public void UpdatePhysics()
        {
            UpdateCoyoteTimer();
            CheckGround();
            HandleHorizontalMovement();
            UpdateGravity();
            MoveCharacter();
            UpdateFacingDirection();

            _wasGrounded = _isGrounded;
        }

        // 코요티 타이머 업데이트
        private void UpdateCoyoteTimer()
        {
            _coyoteTimer = _isGrounded ? _settings.coyoteTime : _coyoteTimer - Time.deltaTime;
        }

        // 지면 감지 (Raycast)
        private void CheckGround()
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _settings.groundCheckDistance, _settings.groundLayer);

            bool wasGrounded = _isGrounded;
            _isGrounded = hit.collider != null;

            // 지면에 있고 떨어지는 중일 때만 처리
            if (_isGrounded && _velocity.y <= 0)
            {
                _velocity.y = 0;
                _jumpCutApplied = false;

                // 지면에 스냅 (착지할 때만, 거리가 유효할 때만)
                if (!wasGrounded && hit.distance > 0.01f)
                {
                    _transform.position += Vector3.down * (hit.distance - 0.01f);
                }
            }
        }

        // 점프 실행 (State에서 호출)
        public void Jump()
        {
            if (CanJump())
            {
                _velocity.y = _settings.jumpForce;
                _coyoteTimer = 0;
                _jumpCutApplied = false;
                _isGrounded = false;
            }
        }

        // 대각선 점프 실행 (점프 공격용)
        public void DiagonalJump(float jumpMultiplier, float forwardSpeed)
        {
            if (CanJump())
            {
                _velocity.y = _settings.jumpForce * jumpMultiplier;
                _velocity.x = forwardSpeed;
                _coyoteTimer = 0;
                _jumpCutApplied = false;
                _isGrounded = false;
            }
        }

        // 점프 컷 (버튼을 떼면 낮은 점프)
        public void CutJump()
        {
            if (_velocity.y > 0 && !_jumpCutApplied)
            {
                _velocity.y *= _settings.jumpCutMultiplier;
                _jumpCutApplied = true;
            }
        }

        // 점프 가능 여부
        public bool CanJump()
        {
            return _coyoteTimer > 0;
        }

        // 수평 이동 처리
        private void HandleHorizontalMovement()
        {
            float targetSpeed = _horizontalInput * (_isGrounded ? _settings.runSpeed : _settings.airSpeed);
            float speedDiff = targetSpeed - _velocity.x;

            // 방향 전환 시 더 빠른 가속
            bool isTurning = (_horizontalInput != 0) && (Mathf.Sign(_horizontalInput) != Mathf.Sign(_velocity.x)) && (Mathf.Abs(_velocity.x) > 0.1f);

            float accelRate;
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                // 가속
                accelRate = _isGrounded ? _settings.groundAcceleration : _settings.airAcceleration;
                if (isTurning)
                {
                    accelRate *= _settings.turnSpeedMultiplier;
                }
            }
            else
            {
                // 감속
                accelRate = _isGrounded ? _settings.groundDeceleration : _settings.airDeceleration;
            }

            // 속도 적용
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

        // 방향 전환 (Scale.x = 1이 오른쪽, -1이 왼쪽)
        private void UpdateFacingDirection()
        {
            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                _facingDirection = (int)Mathf.Sign(_horizontalInput);

                Vector3 scale = _transform.localScale;
                scale.x = _facingDirection;
                _transform.localScale = scale;
            }
        }

        // 벽 감지 (Raycast) - 지면에 걸리지 않도록 위쪽에서 체크
        private bool IsWallHit(float direction)
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.up * _settings.wallCheckHeight;
            Vector2 dir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, _settings.wallCheckDistance, _settings.groundLayer);

            return hit.collider != null;
        }

        // 외부 접근용 메서드
        public Vector2 GetVelocity() => _velocity;
        public bool IsGrounded() => _isGrounded;
        public int GetFacingDirection() => _facingDirection;
        public bool WasJustLanded() => _isGrounded && !_wasGrounded;
        public Vector3 GetPosition() => _transform.position;

        // 방향 설정 (마우스 공격 등에서 사용)
        public void SetFacingDirection(int direction)
        {
            if (direction == 0) return;
            _facingDirection = direction > 0 ? 1 : -1;

            Vector3 scale = _transform.localScale;
            scale.x = _facingDirection;
            _transform.localScale = scale;
        }

        // 외부에서 속도 설정 (대시베기 등에서 사용)
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        // 외부에서 위치 설정 (대시베기 등에서 사용)
        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        // Z축 회전 설정 (대각선 공격 등에서 사용)
        public void SetRotation(float angle)
        {
            _transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 회전 초기화
        public void ResetRotation()
        {
            _transform.rotation = Quaternion.identity;
        }

        // 디버그용 Gizmo 그리기 (Player에서 호출)
        public void DrawGizmos()
        {
            if (_transform == null) return;

            // 지면 체크
            Vector2 groundOrigin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * _settings.groundCheckDistance);

            // 벽 체크
            Vector2 wallOrigin = (Vector2)_transform.position + Vector2.up * _settings.wallCheckHeight;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * _settings.wallCheckDistance);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.left * _settings.wallCheckDistance);
        }
    }
}
