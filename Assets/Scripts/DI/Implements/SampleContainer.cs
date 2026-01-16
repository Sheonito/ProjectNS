using System;
using Aftertime.SecretSome.UI;
using Waving.BlackSpin.FSM;
using Waving.Di;

namespace Waving.BlackSpin.DI
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
