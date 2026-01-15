using SRPG;
using Waving.BlackSpin.DI;
using Waving.Di;

namespace Waving.BlackSpin.FSM
{
    public class SampleState : DIClass,IState
    {
        public OnEnter onEnter { get; set; }
        public OnExecute onExecute { get; set; }
        public OnExit onExit { get; set; }
        
        [Inject] private SampleContainer _container;
        
        public void Enter()
        {
        }

        public void Execute()
        {
            
        }

        public void Exit()
        {
        }
    }
   
}