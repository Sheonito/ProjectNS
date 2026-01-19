using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 백스텝 상태 (뒤로 빠르게 이동)
    public class PlayerBackstepState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _backstepTimer;
        private float _backstepDuration;
        private float _actualBackstepDistance;
        private int _backstepDirection;
        private Vector3 _startPosition;
        private bool _isBackstepping;
        private bool _isRecovering;
        private bool _isCooldownBlocked;

        // 쿨타임 관리 (static)
        private static float _lastBackstepTime = -999f;
        private static float _backstepCooldownDuration;

        public PlayerBackstepState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
            _backstepCooldownDuration = settings.backstepCooldown;
        }

        // 쿨타임 체크 (static - 상태 전환 전 체크용)
        public static bool IsOnCooldownStatic()
        {
            return Time.time - _lastBackstepTime < _backstepCooldownDuration;
        }

        // 쿨타임 체크 (인스턴스용)
        public bool IsOnCooldown()
        {
            return Time.time - _lastBackstepTime < _settings.backstepCooldown;
        }

        public override void Enter()
        {
            base.Enter();
            _backstepTimer = 0;
            _isBackstepping = true;
            _isRecovering = false;
            _isCooldownBlocked = false;

            // 수평 입력 초기화
            _movement.SetHorizontalInput(0);

            // 쿨타임 체크
            if (IsOnCooldown())
            {
                _isCooldownBlocked = true;
                _isBackstepping = false;
                return;
            }

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            float totalDuration = _settings.backstepTargetDuration;
            _backstepDuration = totalDuration * _settings.backstepMoveRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.Backstep);
            float autoSpeedFactor = baseAnimLength / totalDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            // 쿨타임 시작
            _lastBackstepTime = Time.time;

            _startPosition = _movement.GetPosition();

            // 백스텝 방향 (현재 바라보는 방향의 반대)
            _backstepDirection = -_movement.GetFacingDirection();

            // 장애물(벽+경사면) 체크: 거리를 고려해 실제 백스텝 거리 결정
            _actualBackstepDistance = _movement.GetObstacleDistance(_backstepDirection, _settings.backstepDistance);

            // 속도 초기화
            _movement.SetVelocity(Vector2.zero);

            // 백스텝 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerBackstepEvent(_backstepDirection));
        }

        public override void Execute()
        {
            base.Execute();

            // 쿨타임으로 차단된 경우 바로 상태 전환
            if (_isCooldownBlocked)
            {
                ReturnToPreviousState();
                return;
            }

            _backstepTimer += Time.deltaTime;

            // 백스텝 중
            if (_isBackstepping)
            {
                float progress = _backstepTimer / _backstepDuration;

                if (progress < 1f)
                {
                    // 이징 적용 (빠르게 시작, 느리게 끝)
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

                    // X 위치 계산
                    float targetX = _startPosition.x + _backstepDirection * _actualBackstepDistance;
                    float currentX = Mathf.Lerp(_startPosition.x, targetX, easedProgress);

                    // 현재 X 위치에서 지면 Y 가져오기 (경사면 따라 이동)
                    float? groundY = _movement.GetGroundYAtPosition(currentX);
                    float currentY = groundY ?? _startPosition.y;

                    Vector3 currentPosition = new Vector3(currentX, currentY, _startPosition.z);
                    _movement.SetPosition(currentPosition);
                }
                else
                {
                    // 백스텝 완료 → 후딜레이 시작
                    _isBackstepping = false;
                    _isRecovering = true;
                    _backstepTimer = 0;
                }
                return;
            }

            // 후딜레이 중
            if (_isRecovering)
            {
                if (_backstepTimer >= _settings.backstepRecoveryTime)
                {
                    ReturnToPreviousState();
                }
            }
        }

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();

            // 후딜레이 중에만 물리 업데이트
            if (_isRecovering)
            {
                _movement.UpdatePhysics();
            }
        }

        // 이전 상태로 복귀
        private void ReturnToPreviousState()
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
        }

        public override void Exit()
        {
            base.Exit();
            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
