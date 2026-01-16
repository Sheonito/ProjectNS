using Percent111.ProjectNS.DI;
using Percent111.ProjectNS.FSM;

namespace Percent111.ProjectNS.FSM
{
    public class SampleState : IState
    {
        public OnEnter onEnter { get; set; }
        public OnExecute onExecute { get; set; }
        public OnExit onExit { get; set; }
        
        private SampleContainer _container;

        public SampleState()
        {
            _container = DIResolver.Resolve<SampleContainer>();
        }

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
