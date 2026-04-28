using Foundation.FSM;

namespace GamePlay.LauncherFsm
{
    public class LauncherBase : FsmState<LauncherFsm>
    {
        public  float stepDeltaProgress = 0f;
        protected internal override void OnEnter(IFsm<LauncherFsm> fsm)
        { 
            base.OnEnter(fsm);

            fsm.Owner.accumulateProgress = stepDeltaProgress;
        }
 

    }
}