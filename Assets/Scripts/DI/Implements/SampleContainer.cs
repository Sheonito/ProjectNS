using System;
using Aftertime.SecretSome.UI;
using Waving.BlackSpin.FSM;
using Waving.BlackSpin.Slot;
using Waving.Di;

namespace Waving.BlackSpin.DI
{
    public class SampleContainer : DIContainerBase
    {
        public Deck PlayerDeck { get; private set; }
        public ExtendedButton StayButton { get; private set; }
        public ExtendedButton HitButton { get; private set; }

        public SampleContainer(Deck playerDeck,ExtendedButton stayButton,ExtendedButton hitButton)
        {
            PlayerDeck = playerDeck;
            StayButton = stayButton;
            HitButton = hitButton;
        }

        protected override Type GetAllowedType()
        {
            return typeof(PlayerTurnState);
        }
    }   
}
