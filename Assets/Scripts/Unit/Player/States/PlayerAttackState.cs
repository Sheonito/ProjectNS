using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 공격 상태
    public class PlayerAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private float _attackTimer;
        private bool _hasHit;

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

            // 공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerAttackEvent());
        }

        public override void Execute()
        {
            base.Execute();

            float horizontalInput = GetHorizontalInput();

            // 공격 중에도 이동 허용
            _movement.SetHorizontalInput(horizontalInput);
            _movement.UpdatePhysics();

            _attackTimer += Time.deltaTime;

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

        // 공격 판정 처리 (State에서 직접 처리)
        private void PerformAttackHit()
        {
            Vector2 position = _movement.GetPosition();
            int facingDirection = _movement.GetFacingDirection();
            float range = _settings.attackRange;

            Vector2 attackCenter = position + Vector2.right * facingDirection * range * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, range, _settings.enemyLayer);

            foreach (Collider2D hit in hits)
            {
                EnemyUnit enemy = hit.GetComponent<EnemyUnit>();
                enemy?.OnDamaged(_settings.attackDamage);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
