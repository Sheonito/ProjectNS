using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Percent111.ProjectNS.FSM
{

    public class StateTransition
    {
        public IState nextState;
        public Func<bool> condition;
    }
}
