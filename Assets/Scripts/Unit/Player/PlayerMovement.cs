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
        private int _facingDirection = 1;
        private bool _jumpCutApplied;
        private int _jumpCount; // 현재 점프 횟수

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
            CheckGround();
            HandleHorizontalMovement();
            UpdateGravity();
            MoveCharacter();
            SnapToSlope();
            UpdateFacingDirection();

            _wasGrounded = _isGrounded;
        }

        // 지면 감지 (Raycast) + 땅 뚫기 방지
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
                _jumpCutApplied = false;
                return;
            }

            // 2. 정상적인 지면 감지 (아래로 Raycast)
            Vector2 origin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _settings.groundCheckDistance, _settings.groundLayer);

            bool wasGrounded = _isGrounded;
            _isGrounded = hit.collider != null;

            // 지면에 있고 떨어지는 중일 때만 처리
            if (_isGrounded && _velocity.y <= 0)
            {
                _velocity.y = 0;
                _jumpCutApplied = false;
                _jumpCount = 0; // 착지 시 점프 횟수 리셋

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
                // 더블 점프인 경우 힘 배율 적용
                float jumpMultiplier = _jumpCount > 0 ? _settings.doubleJumpMultiplier : 1f;
                _velocity.y = _settings.jumpForce * jumpMultiplier;
                _jumpCutApplied = false;
                _isGrounded = false;
                _jumpCount++;
            }
        }

        // 대각선 점프 실행 (점프 공격용)
        public void DiagonalJump(float jumpMultiplier, float forwardSpeed)
        {
            if (CanJump())
            {
                // 더블 점프인 경우 추가 배율 적용
                float doubleJumpMult = _jumpCount > 0 ? _settings.doubleJumpMultiplier : 1f;
                _velocity.y = _settings.jumpForce * jumpMultiplier * doubleJumpMult;
                _velocity.x = forwardSpeed;
                _jumpCutApplied = false;
                _isGrounded = false;
                _jumpCount++;
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

        // 점프 가능 여부 (지상이거나 더블 점프 횟수 남음)
        public bool CanJump()
        {
            return _isGrounded || _jumpCount < _settings.maxJumpCount;
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

        // 특정 방향으로 벽까지의 거리 반환 (대시/백스텝용)
        public float GetWallDistance(int direction, float maxDistance)
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.up * _settings.wallCheckHeight;
            Vector2 dir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, _settings.groundLayer);

            if (hit.collider != null)
            {
                // 벽까지 거리에서 약간의 여유 공간 확보
                return Mathf.Max(0, hit.distance - 0.1f);
            }
            return maxDistance;
        }

        // 특정 방향으로 장애물(벽+가파른 경사면)까지의 거리 반환 (공격 대시용)
        public float GetObstacleDistance(int direction, float maxDistance)
        {
            float minDistance = maxDistance;
            Vector2 dir = new Vector2(direction, 0);

            // 1. 벽 체크 (위쪽)
            Vector2 wallOrigin = (Vector2)_transform.position + Vector2.up * _settings.wallCheckHeight;
            RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, dir, maxDistance, _settings.groundLayer);
            if (wallHit.collider != null)
            {
                minDistance = Mathf.Min(minDistance, wallHit.distance - 0.1f);
            }

            // 2. 가파른 경사면 체크 (45도 이상만 장애물, 그 이하는 따라 이동 가능)
            Vector2 slopeOrigin = (Vector2)_transform.position + Vector2.down * (_settings.groundCheckOffset - 0.1f);
            RaycastHit2D slopeHit = Physics2D.Raycast(slopeOrigin, dir, maxDistance, _settings.groundLayer);
            if (slopeHit.collider != null)
            {
                float angle = Vector2.Angle(Vector2.up, slopeHit.normal);
                if (angle > 45f)
                {
                    minDistance = Mathf.Min(minDistance, slopeHit.distance - 0.1f);
                }
            }

            // 3. 중간 높이에서도 가파른 경사면/벽 체크
            Vector2 midOrigin = (Vector2)_transform.position;
            RaycastHit2D midHit = Physics2D.Raycast(midOrigin, dir, maxDistance, _settings.groundLayer);
            if (midHit.collider != null)
            {
                float angle = Vector2.Angle(Vector2.up, midHit.normal);
                if (angle > 45f)
                {
                    minDistance = Mathf.Min(minDistance, midHit.distance - 0.1f);
                }
            }

            return Mathf.Max(0, minDistance);
        }

        // 지면까지의 거리 반환 (다이브 공격용)
        public float GetGroundDistance(float maxDistance)
        {
            Vector2 origin = (Vector2)_transform.position + Vector2.down * _settings.groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDistance, _settings.groundLayer);

            if (hit.collider != null)
            {
                return hit.distance;
            }
            return maxDistance;
        }

        // 지면 체크만 수행 (외부에서 호출 가능)
        public void ForceGroundCheck()
        {
            CheckGround();
        }

        // 특정 위치에서 지면 Y 좌표 반환 (대시/백스탭 경사면 이동용)
        // 지면이 없으면 null 반환
        public float? GetGroundYAtPosition(float xPosition)
        {
            Vector2 origin = new Vector2(xPosition, _transform.position.y + 1f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 3f, _settings.groundLayer);

            if (hit.collider != null)
            {
                // 경사각이 45도 이하인 경우에만 유효
                float angle = Vector2.Angle(Vector2.up, hit.normal);
                if (angle <= 45f)
                {
                    return hit.point.y + _settings.groundCheckOffset;
                }
            }
            return null;
        }

        // 지면 레이어 반환 (State에서 직접 Raycast 시 사용)
        public LayerMask GetGroundLayer() => _settings.groundLayer;
        public float GetGroundCheckOffset() => _settings.groundCheckOffset;

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
