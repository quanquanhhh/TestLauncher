using Foundation.FSM;

namespace GamePlay
{
    public class GameFsmBase : FsmState<GameFsm>
    {
        public void ToState<T>(IFsm<GameFsm> owner, params object[] args) where T : GameFsmBase
        {
            ChangeState<T>(owner, args);
        }
 
    }
}