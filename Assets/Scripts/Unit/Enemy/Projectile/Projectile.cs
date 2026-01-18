using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 원거리 적 투사체 (직선 이동)
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private int _damage;
        private float _speed;
        private float _lifeTime;
        private float _timer;
        private Vector2 _direction;
        private ProjectilePool _pool;

        // 초기화 (발사 시 호출)
        public void Initialize(int damage, float speed, float lifeTime, Vector2 direction)
        {
            _damage = damage;
            _speed = speed;
            _lifeTime = lifeTime;
            _direction = direction.normalized;
            _timer = 0;

            // 발사 방향으로 회전
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Pool 설정 (Pool에서 호출)
        public void SetPool(ProjectilePool pool)
        {
            _pool = pool;
        }

        private void Update()
        {
            // 직선 이동
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

            // 생존 시간 체크
            _timer += Time.deltaTime;
            if (_timer >= _lifeTime)
            {
                ReturnToPool();
            }
        }

        // 충돌 처리 (Trigger)
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어 충돌
            if (other.TryGetComponent(out PlayerUnit player))
            {
                // 무적 상태면 통과
                if (player.IsInvincible)
                    return;

                player.OnDamaged(_damage);
                ReturnToPool();
                return;
            }

            // 지형 충돌 (Ground 레이어)
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                ReturnToPool();
            }
        }

        // Pool로 반환
        private void ReturnToPool()
        {
            _pool.Despawn(this);
        }

        // Pool에서 재사용 시 초기화
        public void ResetForPool()
        {
            _timer = 0;
            _direction = Vector2.zero;
        }
    }
}
