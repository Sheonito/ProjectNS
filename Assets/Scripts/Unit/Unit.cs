using UnityEngine;

namespace Percent111.ProjectNS.Unit
{
    // Player와 Enemy의 공통 기본 클래스 (HP 관리만 담당, 상태 전환은 각 클래스에서 처리)
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

        // HP 감소 처리 (상태 전환은 하위 클래스에서 직접 처리)
        public void ApplyDamage(int damage)
        {
            _currentHp -= damage;
            if (_currentHp <= 0)
            {
                _currentHp = 0;
            }
        }

        // 회복 처리
        public void Heal(int amount)
        {
            _currentHp += amount;
            if (_currentHp > _maxHp)
            {
                _currentHp = _maxHp;
            }
        }
    }
}
