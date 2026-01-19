using Percent111.ProjectNS.Effect;
using Percent111.ProjectNS.Event;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    // 히트 이펙트 오브젝트 풀
    public class HitEffectPool : ObjectPool<HitEffect>
    {
        private readonly HitEffect _prefab;

        public HitEffectPool(HitEffect prefab, Transform parent, int preLoadCount = 10)
            : base(parent)
        {
            _prefab = prefab;
            PreLoad(preLoadCount);
            SubscribeEvents();
        }

        // 이벤트 구독
        public void SubscribeEvents()
        {
            EventBus.Subscribe<HitEffectReturnEvent>(OnHitEffectReturn);
        }

        // 이벤트 구독 해제
        public void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<HitEffectReturnEvent>(OnHitEffectReturn);
        }

        // HitEffect 반환 이벤트 핸들러
        private void OnHitEffectReturn(HitEffectReturnEvent evt)
        {
            Despawn(evt.Effect);
        }

        protected override HitEffect CreateItem()
        {
            HitEffect effect = Object.Instantiate(_prefab, _parent);
            return effect;
        }

        protected override void OnDespawn(HitEffect item)
        {
            item.ResetForPool();
        }
    }
}
