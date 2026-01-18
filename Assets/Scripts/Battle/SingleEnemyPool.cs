using Percent111.ProjectNS.Common;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Player;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 단일 타입 적 풀
    public class SingleEnemyPool : ObjectPool<EnemyUnit>
    {
        private readonly GameObject _prefab;
        private readonly PlayerDataProvider _playerData;

        public SingleEnemyPool(GameObject prefab, Transform parent, PlayerDataProvider playerData) 
            : base(parent)
        {
            _prefab = prefab;
            _playerData = playerData;
        }

        protected override EnemyUnit CreateItem()
        {
            GameObject obj = Object.Instantiate(_prefab, _parent);
            return obj.GetComponent<EnemyUnit>();
        }

        protected override void OnSpawn(EnemyUnit item)
        {
            item.SetPlayerData(_playerData);
        }

        protected override void OnDespawn(EnemyUnit item)
        {
            item.ResetForPool();
        }
    }
}
