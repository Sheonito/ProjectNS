using Percent111.ProjectNS.FSM;
using System.Collections.Generic;
namespace Percent111.ProjectNS.FSM
{
    public class SampleStateMachine : StateMachine
    {
        private Dictionary<SampleContentsState, IState> _states;


        public void ChangeState(SampleContentsState state)
        {
            ChangeState(_states[state]);
        }

        public override void Init(IState state)
        {
            _states = new Dictionary<SampleContentsState, IState>();
            SampleState playerTurnState = new SampleState();
            
            _states.Add(SampleContentsState.Sample,playerTurnState);
            
            base.Init(playerTurnState);
            ChangeState(playerTurnState);
        }
    }

    public enum SampleContentsState
    {
        Sample,
    }
}
