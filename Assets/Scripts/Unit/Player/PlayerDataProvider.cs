using System;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    // 플레이어 데이터 제공자 (읽기 전용 + 데미지 적용)
    // Transform 대신 이 클래스를 사용하여 외부에서 플레이어 데이터에 접근
    public class PlayerDataProvider
    {
        private readonly PlayerMovement _movement;
        private readonly Action<int> _applyDamage;

        public PlayerDataProvider(PlayerMovement movement, Action<int> applyDamage)
        {
            _movement = movement;
            _applyDamage = applyDamage;
        }

        // 플레이어 위치 (읽기 전용)
        public Vector2 Position => _movement.GetPosition();

        // 플레이어가 땅에 있는지 여부 (읽기 전용)
        public bool IsGrounded => _movement.IsGrounded();

        // 플레이어에게 데미지 적용
        public void ApplyDamage(int damage)
        {
            _applyDamage?.Invoke(damage);
        }
    }
}
