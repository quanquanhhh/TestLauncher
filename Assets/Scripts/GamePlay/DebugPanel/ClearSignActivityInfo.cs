using GameConfig;

namespace GamePlay.DebugPanel
{
    public class ClearSignActivityInfo : DebugCell
    {
        public override void DebugFun()
        {
            base.DebugFun();
            activityInfo.SignInfo.Clear(); 
        }

        public override string GetDes()
        {
            return "Clear Sign";
        }
    }
}