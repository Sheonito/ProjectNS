using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 카타나 제로 스타일의 플레이어 이동 시스템 (Raycast + Transform 기반)
    public class PlayerMovement : MonoBehaviour
    {
        [Header("이동 속도")]
        [SerializeField] private float _runSpeed = 12f;
        [SerializeField] private float _airSpeed = 10f;

        [Header("가속/감속 (카타나 제로 스타일: 빠른 가속, 즉각적인 정지)")]
        [SerializeField] private float _groundAcceleration = 80f;
        [SerializeField] private float _groundDeceleration = 100f;
        [SerializeField] private float _airAcceleration = 40f;
        [SerializeField] private float _airDeceleration = 30f;
        [SerializeField] private float _turnSpeedMultiplier = 2f;

        [Header("점프")]
        [SerializeField] private float _jumpForce = 18f;
        [SerializeField] private float _gravity = -50f;
        [SerializeField] private float _maxFallSpeed = -30f;
        [SerializeField] private float _coyoteTime = 0.08f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _jumpCutMultiplier = 0.4f;

        [Header("지면 감지")]
        [SerializeField] private float _groundCheckDistance = 0.15f;
        [SerializeField] private float _groundCheckOffset = 0.5f;
        [SerializeField] private LayerMask _groundLayer = 1;

        [Header("벽 감지")]
        [SerializeField] private float _wallCheckDistance = 0.3f;

        private Vector2 _velocity;
        private float _horizontalInput;
        private bool _jumpInputHeld;
        private bool _jumpInputPressed;
        private bool _isGrounded;
        private bool _wasGrounded;
        
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private int _facingDirection = 1;

        private void Update()
        {
            GatherInput();
            UpdateTimers();
            CheckGround();
            HandleJump();
            HandleHorizontalMovement();
            ApplyGravity();
            MoveCharacter();
            UpdateFacingDirection();
            
            _wasGrounded = _isGrounded;
        }

        // 입력 수집
        private void GatherInput()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _jumpInputHeld = Input.GetButton("Jump");
            
            if (Input.GetButtonDown("Jump"))
            {
                _jumpInputPressed = true;
            }
        }

        // 타이머 업데이트
        private void UpdateTimers()
        {
            _coyoteTimer = _isGrounded ? _coyoteTime : _coyoteTimer - Time.deltaTime;
            
            if (_jumpInputPressed)
            {
                _jumpBufferTimer = _jumpBufferTime;
                _jumpInputPressed = false;
            }
            else
            {
                _jumpBufferTimer -= Time.deltaTime;
            }
        }

        // 지면 감지 (Raycast)
        private void CheckGround()
        {
            Vector2 origin = (Vector2)transform.position + Vector2.down * _groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _groundCheckDistance, _groundLayer);
            
            _isGrounded = hit.collider != null;
            
            if (_isGrounded && _velocity.y <= 0)
            {
                _velocity.y = 0;
                
                // 지면에 스냅
                if (hit.distance > 0.01f)
                {
                    transform.position += Vector3.down * (hit.distance - 0.01f);
                }
            }
        }

        // 점프 처리
        private void HandleJump()
        {
            // 점프 버퍼 + 코요티 타임
            if (_jumpBufferTimer > 0 && _coyoteTimer > 0 && _velocity.y <= 0)
            {
                _velocity.y = _jumpForce;
                _jumpBufferTimer = 0;
                _coyoteTimer = 0;
            }

            // 점프 컷 (버튼을 떼면 낮은 점프)
            if (!_jumpInputHeld && _velocity.y > 0)
            {
                _velocity.y *= _jumpCutMultiplier;
            }
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
            transform.position += movement;
        }

        // 방향 전환
        private void UpdateFacingDirection()
        {
            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                _facingDirection = (int)Mathf.Sign(_horizontalInput);
                
                Vector3 scale = transform.localScale;
                scale.x = _facingDirection;
                transform.localScale = scale;
            }
        }

        // 벽 감지 (Raycast)
        private bool IsWallHit(float direction)
        {
            Vector2 origin = transform.position;
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

        // 디버그 시각화
        private void OnDrawGizmosSelected()
        {
            // 지면 체크
            Vector2 groundOrigin = (Vector2)transform.position + Vector2.down * _groundCheckOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * _groundCheckDistance);
            
            // 벽 체크
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.right * _wallCheckDistance);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.left * _wallCheckDistance);
        }
    }
}
