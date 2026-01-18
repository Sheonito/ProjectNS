using System.Collections.Generic;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    // 제네릭 오브젝트 풀 베이스 클래스
    public abstract class ObjectPool<T> where T : Component
    {
        protected readonly Queue<T> _pool;
        protected readonly Transform _parent;

        protected ObjectPool(Transform parent)
        {
            _pool = new Queue<T>();
            _parent = parent;
        }

        // 미리 생성
        public void PreLoad(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = CreateItem();
                item.gameObject.SetActive(false);
                _pool.Enqueue(item);
            }
        }

        // 스폰
        public virtual T Spawn(Vector3 position)
        {
            T item;

            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
            }
            else
            {
                item = CreateItem();
            }

            item.transform.position = position;
            item.gameObject.SetActive(true);
            OnSpawn(item);
            return item;
        }

        // 디스폰
        public virtual void Despawn(T item)
        {
            OnDespawn(item);
            item.gameObject.SetActive(false);
            item.transform.SetParent(_parent);
            _pool.Enqueue(item);
        }

        // 모든 풀 정리
        public void ClearAll()
        {
            while (_pool.Count > 0)
            {
                T item = _pool.Dequeue();
                if (item != null)
                {
                    Object.Destroy(item.gameObject);
                }
            }
        }

        // 하위 클래스에서 구현
        protected abstract T CreateItem();

        // 스폰/디스폰 시 추가 처리 (선택적 오버라이드)
        protected virtual void OnSpawn(T item) { }
        protected virtual void OnDespawn(T item) { }
    }
}
