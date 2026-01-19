using DG.Tweening;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Item
{
    // 방어막 아이템 (적 사망 시 드롭, 플레이어에게 흡수)
    public class ShieldItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _attractSpeed = 15f;
        [SerializeField] private float _attractAcceleration = 20f;
        [SerializeField] private float _collectDistance = 0.5f;
        [SerializeField] private float _initialScatterForce = 5f;
        [SerializeField] private float _scatterDuration = 0.3f;

        private Transform _targetPlayer;
        private bool _isAttracting;
        private float _currentSpeed;
        private Vector2 _scatterVelocity;
        private float _scatterTimer;

        private void OnEnable()
        {
            _isAttracting = false;
            _currentSpeed = 0f;
            _scatterTimer = 0f;
            
            // 초기 흩어짐 방향 설정 (랜덤)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _scatterVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _initialScatterForce;
            
            // 페이드 인
            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = 0f;
                _spriteRenderer.color = c;
                _spriteRenderer.DOFade(1f, 0.2f);
            }
        }

        private void Update()
        {
            if (_targetPlayer == null)
            {
                FindPlayer();
                return;
            }

            // 초기 흩어짐 (잠시 바깥으로 튀었다가)
            if (_scatterTimer < _scatterDuration)
            {
                _scatterTimer += Time.deltaTime;
                _scatterVelocity = Vector2.Lerp(_scatterVelocity, Vector2.zero, Time.deltaTime * 5f);
                transform.position += (Vector3)_scatterVelocity * Time.deltaTime;
                return;
            }

            // 플레이어에게 흡수
            _isAttracting = true;
            Vector3 direction = (_targetPlayer.position - transform.position).normalized;
            _currentSpeed += _attractAcceleration * Time.deltaTime;
            _currentSpeed = Mathf.Min(_currentSpeed, _attractSpeed);

            transform.position += direction * _currentSpeed * Time.deltaTime;

            // 수집 거리 도달
            float distance = Vector3.Distance(transform.position, _targetPlayer.position);
            if (distance <= _collectDistance)
            {
                OnCollected();
            }
        }

        private void FindPlayer()
        {
            PlayerUnit playerObj = FindAnyObjectByType<PlayerUnit>();
            if (playerObj != null)
            {
                _targetPlayer = playerObj.transform;
            }
        }

        private void OnCollected()
        {
            // 방어막 획득 이벤트 발행
            EventBus.Publish(this, new ShieldCollectedEvent());

            // 페이드 아웃 후 풀로 반환
            if (_spriteRenderer != null)
            {
                _spriteRenderer.DOFade(0f, 0.1f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // 풀에서 초기화 시 호출
        public void ResetForPool()
        {
            _isAttracting = false;
            _currentSpeed = 0f;
            _scatterTimer = 0f;
            _targetPlayer = null;
        }
    }
}
