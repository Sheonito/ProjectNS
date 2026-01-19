using Percent111.ProjectNS.FSM;
using System;
using System.Collections.Generic;
namespace Percent111.ProjectNS.Event
{
    public class SampleEvent : IEvent
    {
        public bool IsDraw { get; private set; }
        public List<int> Damages { get; private set; }
        public int AttackTotal { get; private set; }
        public int HP { get; private set; }
        public Type DamagedType { get; private set; }
        
        public SampleEvent(bool isDraw,List<int> damages, int attackTotal,int hp, Type damagedType)
        {
            IsDraw = isDraw;
            Damages = damages;
            AttackTotal = attackTotal;
            HP = hp;
            DamagedType = damagedType;
        }

        public Type GetPublishType()
        {
            // return typeof(SampleState);
            return null;
        }
    }
   
}
