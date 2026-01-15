using System;

namespace Waving.Common.Event
{
    public interface IEvent
    {
        public Type GetPublishType();
    }
   
}