using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SRPG;
using UnityEngine;

namespace StateMachine.Runtime
{
    public class StateMachine
    {
        // 정적 이벤트 - 인스턴스 추적용
        public static event Action<StateMachine> OnInstanceCreated;
        public static event Action<StateMachine> OnInstanceDestroyed;
        
        // 인스턴스 이벤트 - 상태 변경 추적용
        public event Action<IState, IState> OnStateChanged;
        
        public IState CurState { get; private set; }
        public IState PreState { get; private set; }
        public IState GlobalState { get; private set; }
        public string Name { get; private set; }
        
        private List<StateTransition> _transitions;

        public virtual void Init(IState state)
        {
            Name = GetType().Name;
            CurState = state;
            PreState = null;
            GlobalState = null;
            _transitions = new List<StateTransition>();
            
            InvokeOnInstanceCreated();
            CurState.Enter();
        }

        // 유니티 Update에서 매 프레임 호출
        public virtual void Execute()
        {
            StateTransition transition = _transitions.SingleOrDefault(transiton => transiton.condition?.Invoke() == true);
            if (transition != null)
            {
                ChangeState(transition.nextState);
                return;
            }

            if (GlobalState != null)
            {
                GlobalState.Execute();
            }

            if (CurState != null)
            {
                CurState.Execute();
            }
        }
        
        public virtual void ChangeState(IState newState)
        {
            IState previousState = CurState;
            
            if (CurState != null)
            {
                PreState = CurState;
                CurState.Exit();
            }

            CurState = newState;
            CurState.Enter();
            
            InvokeOnStateChanged(previousState, newState);
        }

        public virtual void SetGlobalState(IState newState)
        {
            GlobalState = newState;
            GlobalState.Enter();
        }

        public virtual void BackToPreState()
        {
            ChangeState(PreState);
        }

        public virtual void AddTransition(StateTransition transition)
        {
            _transitions.Add(transition);
        }

        public virtual void RemoveTransition(StateTransition transition)
        {
            if (_transitions.Contains(transition))
                _transitions.Remove(transition);
        }
        
        // 이벤트 발생 시 에러가 발생해도 Runtime에 영향 없도록 try-catch 처리
        private void InvokeOnInstanceCreated()
        {
            try
            {
                OnInstanceCreated?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        // 이벤트 발생 시 에러가 발생해도 Runtime에 영향 없도록 try-catch 처리
        private void InvokeOnInstanceDestroyed()
        {
            try
            {
                OnInstanceDestroyed?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        // 이벤트 발생 시 에러가 발생해도 Runtime에 영향 없도록 try-catch 처리
        private void InvokeOnStateChanged(IState previousState, IState newState)
        {
            try
            {
                OnStateChanged?.Invoke(previousState, newState);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }   
}