using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 이동 물리 처리 (일반 C# 클래스, State에서 호출하여 사용)
    public class PlayerMovement
    {
        // 이동 속도
        private float _runSpeed = 12f;
        private float _airSpeed = 10f;

        // 가속/감속 (카타나 제로 스타일: 빠른 가속, 즉각적인 정지)
        private float _groundAcceleration = 80f;
        private float _groundDeceleration = 100f;
        private float _airAcceleration = 40f;
        private float _airDeceleration = 30f;
        private float _turnSpeedMultiplier = 2f;

        // 점프
        private float _jumpForce = 18f;
        private float _gravity = -50f;
        private float _maxFallSpeed = -30f;
        private float _coyoteTime = 0.08f;
        private float _jumpCutMultiplier = 0.4f;

        // 지면 감지
        private float _groundCheckDistance = 0.15f;
        private float _groundCheckOffset = 0.5f;
        private LayerMask _groundLayer;

        // 벽 감지
        private float _wallCheckDistance = 0.3f;
        private float _wallCheckHeight = 0.3f;

        // 참조
        private Transform _transform;

        // 상태
        private Vector2 _velocity;
        private float _horizontalInput;
        private bool _isGrounded;
        private bool _wasGrounded;
        private float _coyoteTimer;
        private int _facingDirection = 1;
        private bool _jumpCutApplied;

        // 생성자
        public PlayerMovement(Transform transform, LayerMask groundLayer)
        {
            _transform = transform;
            _groundLayer = groundLayer;
        }

        // 설정 변경용 메서드
        public void SetMovementSettings(float runSpeed, float airSpeed, float groundAccel, float groundDecel, float airAccel, float airDecel, float turnMultiplier)
        {
            _runSpeed = runSpeed;
            _airSpeed = airSpeed;
            _groundAcceleration = groundAccel;
            _groundDeceleration = groundDecel;
            _airAcceleration = airAccel;
            _airDeceleration = airDecel;
            _turnSpeedMultiplier = turnMultiplier;
        }

        public void SetJumpSettings(float jumpForce, float gravity, float maxFallSpeed, float coyoteTime, float jumpCutMultiplier)
        {
            _jumpForce = jumpForce;
            _gravity = gravity;
            _maxFallSpeed = maxFallSpeed;
            _coyoteTime = coyoteTime;
            _jumpCutMultiplier = jumpCutMultiplier;
        }

        public void SetGroundCheckSettings(float distance, float offset)
        {
            _groundCheckDistance = distance;
            _groundCheckOffset = offset;
        }

        public void SetWallCheckSettings(float distance, float height)
        {
            _wallCheckDistance = distance;
            _wallCheckHeight = height;
        }

        // 수평 입력 설정 (State에서 호출)
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
            ApplyGravity();
            MoveCharacter();
            UpdateFacingDirection();

            _wasGrounded = _isGrounded;
        }

        // 코요티 타이머 업데이트
        private void UpdateCoyoteTimer()
        {
            _coyoteTimer = _isGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;
        }

        // 지면 감지 (Raycast)
        private void CheckGround()
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.down * _groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _groundCheckDistance, _groundLayer);

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
                _velocity.y = _jumpForce;
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
                _velocity.y *= _jumpCutMultiplier;
                _jumpCutApplied = true;
            }
        }

        // 점프 가능 여부
        public bool CanJump()
        {
            return _coyoteTimer > 0;
        }

        // 수평 이동 처리 (카타나 제로 스타일)
        private void HandleHorizontalMovement()
        {
            float targetSpeed = _horizontalInput * (_isGrounded ? _runSpeed : _airSpeed);
            float speedDiff = targetSpeed - _velocity.x;

            // 방향 전환 시 더 빠른 가속
            bool isTurning = (_horizontalInput != 0) && (Mathf.Sign(_horizontalInput) != Mathf.Sign(_velocity.x)) && (Mathf.Abs(_velocity.x) > 0.1f);

            float accelRate;
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                // 가속
                accelRate = _isGrounded ? _groundAcceleration : _airAcceleration;
                if (isTurning)
                {
                    accelRate *= _turnSpeedMultiplier;
                }
            }
            else
            {
                // 감속
                accelRate = _isGrounded ? _groundDeceleration : _airDeceleration;
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
        private void ApplyGravity()
        {
            if (!_isGrounded)
            {
                _velocity.y += _gravity * Time.deltaTime;
                _velocity.y = Mathf.Max(_velocity.y, _maxFallSpeed);
            }
        }

        // 이동 적용
        private void MoveCharacter()
        {
            Vector3 movement = new Vector3(_velocity.x, _velocity.y, 0) * Time.deltaTime;
            _transform.position += movement;
        }

        // 방향 전환 (Scale.x = 1이 왼쪽, -1이 오른쪽)
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

        // 벽 감지 (Raycast) - 지면에 걸리지 않도록 위쪽에서 체크
        private bool IsWallHit(float direction)
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.up * _wallCheckHeight;
            Vector2 dir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, _wallCheckDistance, _groundLayer);

            return hit.collider != null;
        }

        // 외부 접근용 메서드
        public Vector2 GetVelocity() => _velocity;
        public bool IsGrounded() => _isGrounded;
        public int GetFacingDirection() => _facingDirection;
        public bool WasJustLanded() => _isGrounded && !_wasGrounded;

        // 외부에서 속도 설정 (대시베기 등에서 사용)
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        // 디버그용 Gizmo 그리기 (Player에서 호출)
        public void DrawGizmos()
        {
            if (_transform == null) return;

            // 지면 체크
            Vector2 groundOrigin = (Vector2)_transform.position + Vector2.down * _groundCheckOffset;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * _groundCheckDistance);

            // 벽 체크
            Vector2 wallOrigin = (Vector2)_transform.position + Vector2.up * _wallCheckHeight;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * _wallCheckDistance);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.left * _wallCheckDistance);
        }
    }
}
