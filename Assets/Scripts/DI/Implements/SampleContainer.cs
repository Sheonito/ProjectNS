using System;
using Percent111.ProjectNS.FSM;
using Percent111.ProjectNS.UI;

namespace Percent111.ProjectNS.DI
{
    public class SampleContainer : DIContainerBase
    {
        public ExtendedButton StayButton { get; private set; }
        public ExtendedButton HitButton { get; private set; }

        public SampleContainer(ExtendedButton stayButton, ExtendedButton hitButton)
        {
            StayButton = stayButton;
            HitButton = hitButton;
        }

        public override Type GetAllowedType()
        {
            // return typeof(SampleState);
            return null;
        }
    }   
}
