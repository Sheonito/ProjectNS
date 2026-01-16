using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waving.Di
{
    // 컨테이너 등록 및 Resolve 담당
    public static class DIResolver
    {
        private static List<DIContainerBase> _containers = new List<DIContainerBase>();
        
        // 컨테이너 등록
        public static void Register(DIContainerBase container)
        {
            if (!_containers.Contains(container))
            {
                _containers.Add(container);
            }
        }
        
        // 컨테이너 해제
        public static void Unregister(DIContainerBase container)
        {
            _containers.Remove(container);
        }
        
        // 타입으로 컨테이너 직접 가져오기
        public static T Resolve<T>() where T : DIContainerBase
        {
            foreach (DIContainerBase container in _containers)
            {
                if (container is T typedContainer)
                {
                    return typedContainer;
                }
            }
            
            return null;
        }
        
        // 종속 주입
        public static void Inject(object instance)
        {
            Type instanceType = instance.GetType();
            
            foreach (DIContainerBase container in _containers)
            {
                Type allowedType = container.GetAllowedType();
                if (!IsAllowedType(allowedType, instanceType))
                    continue;
                
                InjectContainer(container, instance);
            }
        }
        
        // 허용된 타입인지 확인
        private static bool IsAllowedType(Type allowedType, Type type)
        {
            return allowedType == type || allowedType.IsAssignableFrom(type);
        }
        
        // 실제 Inject 실행
        private static void InjectContainer(DIContainerBase container, object target)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            
            foreach (FieldInfo field in target.GetType().GetFields(flags))
            {
                if (field.IsDefined(typeof(InjectAttribute), true))
                {
                    if (field.FieldType.IsAssignableFrom(container.GetType()))
                    {
                        field.SetValue(target, container);
                    }
                }
            }
        }
        
        // 모든 컨테이너 초기화
        public static void Clear()
        {
            _containers.Clear();
        }
        
        // 등록된 컨테이너 개수
        public static int GetContainerCount()
        {
            return _containers.Count;
        }
    }
}
