namespace GamePlay.DebugPanel
{
    public class Activity_LuckyWheel : DebugCell
    {
        public override string GetDes()
        {
            return "AddLuckyWheelRollingTime";
        }
        public override void DebugFun()
        {
            base.DebugFun();
            activityInfo.LuckyWheelInfo.RollingTimes = 999;
        }

    }
}