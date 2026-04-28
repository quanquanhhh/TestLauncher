using System;
using Foundation;
using Foundation.FSM;

namespace GamePlay
{
    public class GameFsm : SingletonScript<GameFsm>
    {
        public Type[] states =
        {
            typeof(GameStateLobby),
            typeof(GameStatePlay)
        };

        private IFsmManager _manager;
        private IFsm<GameFsm> _fsm;
        public object[] stateEnterParams;
        public void OnInit()
        {
            _manager = new FsmManager();
            FsmState<GameFsm>[] fsms = new FsmState<GameFsm>[states.Length] ;

            for (int i = 0; i < states.Length; i++)
            {
                fsms[i] = (FsmState<GameFsm>) Activator.CreateInstance(states[i]);
            }

            _fsm = _manager.CreateFsm("GameFsm", this, fsms);
        }

        public bool InGame()
        {
            return _fsm.CurrentState is GameStatePlay;
        }
        public void ToState<T>(params object[] args) where T : GameFsmBase
        {
            stateEnterParams = args;
            if (_fsm.CurrentState == null)
            {
                _fsm.Start<T>();
            }
            else
            {
                if (_fsm.CurrentState is T)
                {
                    _fsm.CurrentState.OnResetState();
                }
                else
                {
                    ((GameFsmBase) _fsm.CurrentState).ToState<T>(_fsm,stateEnterParams);
                }
            }
        }
        
    }
}