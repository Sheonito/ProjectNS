using System;
using Percent111.ProjectNS.DI.System;
using Percent111.ProjectNS.FSM.States;
using Percent111.ProjectNS.UI.Button.System;

namespace Percent111.ProjectNS.DI.Implements
{
    public class SampleContainer : DIContainerBase
    {
        public ExtendedButton StayButton { get; private set; }
        public ExtendedButton HitButton { get; private set; }

        public SampleContainer(ExtendedButton stayButton,ExtendedButton hitButton)
        {
            StayButton = stayButton;
            HitButton = hitButton;
        }

        public override Type GetAllowedType()
        {
            return typeof(SampleState);
        }
    }   
}
