using Percent111.ProjectNS.DI.Implements;
using Percent111.ProjectNS.DI.System;
using Percent111.ProjectNS.FSM.System;
namespace Percent111.ProjectNS.FSM.States
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
