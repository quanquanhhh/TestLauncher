using System.Threading.Tasks;
using UnityEngine;

namespace Foundation.FSM
{
    public abstract class FsmState<T> where T : class
    {
        
        public object[] stateEnterParams;
        public FsmState()
        {
        }
        protected internal virtual void OnInit(IFsm<T> fsm)
        {
        }
        protected internal virtual void OnEnter(IFsm<T> fsm)
        {
        }

        protected internal virtual async Task<bool> PreOnEnter(IFsm<T> fsm)
        {
            return true;
        }
        protected internal virtual void OnUpdate(IFsm<T> fsm, float elapseSeconds, float realElapseSeconds)
        {
        }

        protected internal virtual async Task<bool> PreOnLeave(IFsm<T> fsm, FsmState<T> state)
        {
            return true;
        }
        protected internal virtual void OnLeave(IFsm<T> fsm, bool isShutdown)
        {
        }
        protected internal virtual void OnDestroy(IFsm<T> fsm)
        {
        }
        protected void ChangeState<TState>(IFsm<T> fsm, params object[] args) where TState : FsmState<T>
        {
            Fsm<T> fsmImplement = (Fsm<T>)fsm;
            if (fsmImplement == null)
            {
                throw new UnityException("FSM is invalid.");
            }

            stateEnterParams = args;
            fsmImplement.ChangeState<TState>();
        }

        protected internal virtual void OnResetState()
        { 
        }
    }
}