using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Percent111.ProjectNS.FSM
{
    public delegate void OnEnter();

    public delegate void OnExecute();

    public delegate void OnExit();

    public delegate void OnExecutePhysics();
    
    public interface IState
    {

        public OnEnter onEnter { get; set; }
        public OnExecute onExecute { get; set; }
        public OnExit onExit { get; set; }
        public OnExecutePhysics onExecutePhysics { get; set; }

        // 시작할 때 1회 호출
        public void Enter();
    
        // Update에서 매 프레임 호출 (입력 처리)
        public void Execute();

        // FixedUpdate에서 매 프레임 호출 (물리 처리)
        public void ExecutePhysics();

        // 종료할 때 1회 호출
        public void Exit();
    }
}


