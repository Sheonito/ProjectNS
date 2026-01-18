using Percent111.ProjectNS.Common;
using UnityEngine;

namespace Percent111.ProjectNS.Enemy
{
    // 투사체 오브젝트 풀
    public class ProjectilePool : ObjectPool<Projectile>
    {
        private readonly Projectile _prefab;

        public ProjectilePool(Projectile prefab, Transform parent, int preLoadCount = 10) 
            : base(parent)
        {
            _prefab = prefab;
            PreLoad(preLoadCount);
        }

        protected override Projectile CreateItem()
        {
            Projectile projectile = Object.Instantiate(_prefab, _parent);
            projectile.SetPool(this);
            return projectile;
        }

        protected override void OnDespawn(Projectile item)
        {
            item.ResetForPool();
        }
    }
}
