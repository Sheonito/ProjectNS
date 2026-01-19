using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 공격 상태 (슬래시 공격 - 살짝 대시 포함, 다수 공격)
    public class PlayerAttackState : PlayerStateBase
    {
        private readonly PlayerMovement _movement;
        private readonly PlayerStateSettings _settings;
        private readonly PlayerAnimator _animator;
        private float _attackTimer;
        private float _attackDuration;
        private float _slashDuration;
        private float _actualSlashDistance;
        private float _hitTiming;
        private bool _hasHit;
        private int _attackDirection;
        private Vector3 _startPosition;
        private bool _isSlashing;
        private CancellationTokenSource _hitCts;

        public PlayerAttackState(PlayerMovement movement, PlayerStateSettings settings, PlayerAnimator animator) : base()
        {
            _movement = movement;
            _settings = settings;
            _animator = animator;
        }

        public override void Enter()
        {
            base.Enter();
            _attackTimer = 0;
            _hasHit = false;
            _startPosition = _movement.GetPosition();

            // 이전 타격 취소
            _hitCts?.Cancel();
            _hitCts?.Dispose();
            _hitCts = new CancellationTokenSource();

            // 목표 duration 기반 계산 (애니메이션 속도 자동 조절)
            _attackDuration = _settings.attackTargetDuration;
            _slashDuration = _attackDuration * _settings.slashDashRatio;
            _hitTiming = _attackDuration * _settings.attackHitTimingRatio;

            // 애니메이션 속도 자동 계산 (애니메이션 길이 / 목표 시간)
            float baseAnimLength = _animator.GetAnimationLength(PlayerStateType.Attack);
            float autoSpeedFactor = baseAnimLength / _attackDuration;
            _animator.SetAnimationSpeed(autoSpeedFactor);

            // 수평 입력 초기화 (이전 입력 제거)
            _movement.SetHorizontalInput(0);

            // 마우스 방향에 따라 플레이어 방향 설정
            Vector2 playerPos = _movement.GetPosition();
            _attackDirection = GetMouseHorizontalDirection(playerPos);
            _movement.SetFacingDirection(_attackDirection);

            // 적이 공격 범위 내에 있으면 제자리 공격, 없으면 슬래시 대시
            bool hasEnemyInRange = IsEnemyInAttackRange(playerPos, _settings.attackRange, _settings.enemyLayer);
            _isSlashing = !hasEnemyInRange && _movement.IsGrounded();

            // 슬래시 대시 시 장애물(벽+경사면) 체크
            if (_isSlashing)
            {
                _actualSlashDistance = _movement.GetObstacleDistance(_attackDirection, _settings.slashDashDistance);
            }

            // 공격 이벤트 발행 (사운드, 이펙트 등)
            EventBus.Publish(this, new PlayerAttackEvent());
        }

        public override void Execute()
        {
            base.Execute();

            _attackTimer += Time.deltaTime;

            // 슬래시 대시 중 (살짝 앞으로 이동) - 직접 위치 제어
            if (_isSlashing && _attackTimer < _slashDuration)
            {
                float progress = _attackTimer / _slashDuration;

                // X 위치 계산
                float targetX = _startPosition.x + _attackDirection * _actualSlashDistance;
                float currentX = Mathf.Lerp(_startPosition.x, targetX, progress);

                // 현재 X 위치에서 지면 Y 가져오기 (경사면 따라 이동)
                float? groundY = _movement.GetGroundYAtPosition(currentX);
                float currentY = groundY ?? _startPosition.y;

                Vector3 currentPosition = new Vector3(currentX, currentY, _startPosition.z);
                _movement.SetPosition(currentPosition);
            }
            else
            {
                _isSlashing = false;
            }

            // 공격 판정 타이밍
            if (!_hasHit && _attackTimer >= _hitTiming)
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

            // 백스텝 입력 시 (지상 + 적이 가까이 있을 때만)
            if (IsBackstepPressed() && _movement.IsGrounded() && !_isSlashing)
            {
                RequestStateChange(PlayerStateType.Backstep);
                return;
            }

            // 공격 완료
            if (_attackTimer >= _attackDuration)
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

        public override void ExecutePhysics()
        {
            base.ExecutePhysics();

            // 슬래시 완료 후에만 물리 업데이트
            if (!_isSlashing)
            {
                _movement.UpdatePhysics();
            }
        }

        // 공격 판정 처리 (다수 공격, 순차 타격감)
        private void PerformAttackHit()
        {
            Vector2 position = _movement.GetPosition();
            Vector2 attackDirection = GetMouseDirection(position);
            float range = _settings.attackRange;

            // 공격 방향 중심으로 탐색
            Vector2 attackCenter = position + attackDirection * range * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, range, _settings.enemyLayer);

            // 공격 방향에 있는 적만 수집 (뒤에 있는 적 제외)
            List<EnemyUnit> enemies = new List<EnemyUnit>();
            foreach (Collider2D hit in hits)
            {
                EnemyUnit enemy = hit.GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    // 적이 공격 방향에 있는지 확인 (수평 방향 기준)
                    Vector2 toEnemy = (Vector2)enemy.transform.position - position;
                    float horizontalDir = Mathf.Sign(attackDirection.x);
                    
                    // 공격 방향과 같은 쪽에 있는 적만 포함
                    if (Mathf.Sign(toEnemy.x) == horizontalDir || Mathf.Abs(toEnemy.x) < 0.1f)
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            // 거리순 정렬
            enemies.Sort((a, b) =>
            {
                float distA = Vector2.Distance(position, a.transform.position);
                float distB = Vector2.Distance(position, b.transform.position);
                return distA.CompareTo(distB);
            });

            // 순차 타격 (비동기)
            if (enemies.Count > 0)
            {
                PerformSequentialHitsAsync(enemies, _hitCts.Token).Forget();
            }
        }

        // 순차 타격 (손에 걸리는 듯한 타격감 + 히트스탑)
        private async UniTaskVoid PerformSequentialHitsAsync(List<EnemyUnit> enemies, CancellationToken ct)
        {
            float hitInterval = _settings.hitInterval;

            foreach (EnemyUnit enemy in enemies)
            {
                if (ct.IsCancellationRequested) return;
                if (enemy == null) continue;

                // 데미지 적용
                enemy.OnDamaged(_settings.attackDamage);

                // 히트스탑 + 카메라쉐이크 + 이펙트 (적 위치에)
                HitEffectManager.Instance?.PlayHitFeedback(enemy.transform.position);

                // 다음 적까지 간격 (마지막 적이 아니면)
                if (enemy != enemies[enemies.Count - 1])
                {
                    // realtime으로 대기 (히트스탑 중에도 진행)
                    await UniTask.Delay((int)(hitInterval * 1000), DelayType.Realtime, cancellationToken: ct);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 순차 타격은 상태 전환 후에도 계속 진행 (Cancel하지 않음)
            // 새 공격 시작 시 Enter()에서 이전 타격 취소됨

            // 애니메이션 속도 복원
            _animator.ResetAnimationSpeed();
        }
    }
}
