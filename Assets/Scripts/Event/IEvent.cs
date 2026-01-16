using System;

namespace Percent111.ProjectNS.Event
{
    public interface IEvent
    {
        public Type GetPublishType();
    }
   
}