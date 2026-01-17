using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 대시베기 상태
    public class PlayerDashAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private float _dashTimer;
        private int _dashDirection;
        private Vector3 _startPosition;
        private bool _hasHit;

        public PlayerDashAttackState(PlayerMovement movement, PlayerStateSettings settings) : base()
        {
            _movement = movement;
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _dashTimer = 0;
            _hasHit = false;
            _startPosition = _movement.GetPosition();

            // 대시 방향 결정 (현재 바라보는 방향)
            _dashDirection = _movement.GetFacingDirection();

            // 대시공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerDashAttackEvent(_dashDirection));
        }

        public override void Execute()
        {
            base.Execute();

            _dashTimer += Time.deltaTime;
            float progress = _dashTimer / _settings.dashDuration;

            // 대시 이동 (Lerp로 부드럽게)
            if (progress < 1f)
            {
                Vector3 targetPosition = _startPosition + Vector3.right * _dashDirection * _settings.dashDistance;
                Vector3 currentPosition = Vector3.Lerp(_startPosition, targetPosition, progress);
                _movement.SetPosition(currentPosition);

                // 대시 중 공격 판정 (1회만, 1명만)
                if (!_hasHit)
                {
                    PerformDashAttackHit();
                }
            }

            // 대시 완료
            if (_dashTimer >= _settings.dashDuration)
            {
                // 착지 여부에 따라 상태 전환
                _movement.UpdatePhysics();

                if (!_movement.IsGrounded())
                {
                    RequestStateChange(PlayerStateType.Jump);
                }
                else
                {
                    RequestStateChange(PlayerStateType.Idle);
                }
                return;
            }
        }

        // 대시 공격 판정 처리 (State에서 직접 처리, 1명 제한)
        private void PerformDashAttackHit()
        {
            Vector2 position = _movement.GetPosition();
            float dashDistance = _settings.dashDistance;

            Vector2 attackCenter = position + Vector2.right * _dashDirection * dashDistance * 0.5f;
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackCenter,
                new Vector2(dashDistance, 1f),
                0f,
                _settings.enemyLayer
            );

            // 1명만 타격
            if (hits.Length > 0)
            {
                EnemyUnit enemy = hits[0].GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    enemy.OnDamaged(_settings.dashDamage);
                    _hasHit = true;
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
