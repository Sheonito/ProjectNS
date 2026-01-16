using UnityEngine;

namespace Percent111.ProjectNS.Unit
{
    // Player와 Enemy의 공통 기본 클래스
    public abstract class Unit : MonoBehaviour
    {
        [Header("기본 스탯")]
        [SerializeField] protected int _maxHp = 100;
        [SerializeField] protected int _currentHp;

        public int MaxHp => _maxHp;
        public int CurrentHp => _currentHp;
        public bool IsDead => _currentHp <= 0;

        protected virtual void Awake()
        {
            _currentHp = _maxHp;
        }

        // 데미지 처리
        public virtual void TakeDamage(int damage)
        {
            _currentHp -= damage;
            if (_currentHp <= 0)
            {
                _currentHp = 0;
                OnDeath();
            }
        }

        // 회복 처리
        public virtual void Heal(int amount)
        {
            _currentHp += amount;
            if (_currentHp > _maxHp)
            {
                _currentHp = _maxHp;
            }
        }

        // 사망 시 호출
        protected abstract void OnDeath();
    }
}
