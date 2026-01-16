using System;

namespace Percent111.ProjectNS.DI.System
{
    public abstract class DIContainerBase
    {
        // DIResolver에서 허용 타입 확인용
        public abstract Type GetAllowedType();

        protected DIContainerBase()
        {
            DIResolver.Register(this);
        }
    }
}