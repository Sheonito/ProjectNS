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
        private float _attackTimer;
        private bool _hasHit;
        private int _attackDirection;
        private Vector3 _startPosition;
        private bool _isSlashing;

        public PlayerAttackState(PlayerMovement movement, PlayerStateSettings settings) : base()
        {
            _movement = movement;
            _settings = settings;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasHit = false;
            _isSlashing = true;
            _startPosition = _movement.GetPosition();

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 마우스 방향에 따라 플레이어 방향 설정
            Vector2 playerPos = _movement.GetPosition();
            _attackDirection = GetMouseHorizontalDirection(playerPos);
            _movement.SetFacingDirection(_attackDirection);

            // 공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerAttackEvent());
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;

            // 슬래시 대시 중 (살짝 앞으로 이동)
            if (_isSlashing && _attackTimer < _settings.slashDashDuration)
            {
                float progress = _attackTimer / _settings.slashDashDuration;
                Vector3 targetPosition = _startPosition + Vector3.right * _attackDirection * _settings.slashDashDistance;
                Vector3 currentPosition = Vector3.Lerp(_startPosition, targetPosition, progress);
                _movement.SetPosition(currentPosition);
            }
            else
            {
                _isSlashing = false;
                // 슬래시 완료 후 물리 업데이트
                _movement.UpdatePhysics();
            }

            // 공격 판정 타이밍
            if (!_hasHit && _attackTimer >= _settings.attackHitTiming)
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
            if (_attackTimer >= _settings.attackDuration)
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
        }
    }
}
