using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waving.Di
{
    public abstract class DIContainerBase
    {
        protected abstract Type GetAllowedType();        
        
        private static List<DIContainerBase> _containers = new();

        protected DIContainerBase()
        {
            _containers.Add(this);
        }

        // 외부 객체가 생성될 때 자동으로 호출됨 (DIClass 덕분)
        public static void TryInjectAll(object instance)
        {
            Type instanceType = instance.GetType();

            foreach (var container in _containers)
            {
                // ForType와 하위타입까지 허용
                Type allowedType = container.GetAllowedType();
                if (!container.IsAllowedType(allowedType,instanceType))
                    continue;

                container.Inject(instance);
            }
        }

        private bool IsAllowedType(Type allowedType,Type type)
        {
            return allowedType == type || allowedType.IsAssignableFrom(type);
        }

        // 실제 Inject 실행
        private void Inject(object target)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in target.GetType().GetFields(flags))
            {
                if (field.IsDefined(typeof(InjectAttribute), true))
                {
                    // 주입 대상이 "컨테이너 자신"일 때만 주입
                    if (field.FieldType.IsAssignableFrom(GetType()))
                    {
                        field.SetValue(target, this);
                    }
                }
            }
        }
    }
}