using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;

namespace GamePlay.DebugPanel
{
    public class ClearPassActivityInfo : DebugCell
    {
        public override void DebugFun()
        {
            base.DebugFun();
            activityInfo.PassInfo.Clear();
            GameConfigSys.DealPassImage();
        }

        public override string GetDes()
        {
            return "Clear Pass";
        }
    }
    public class ClearSecretActivityInfo : DebugCell
    {
        public override void DebugFun()
        {
            base.DebugFun();
            activityInfo.SecretGiftInfo.Clear(); 
        }

        public override string GetDes()
        {
            return "Clear Secret";
        }
    }
    public class ClearDailyChallengeInfo : DebugCell
    {
        public override void DebugFun()
        {
            base.DebugFun();
            StorageManager.Instance.GetStorage<DailyInfo>().Clear();
        }

        public override string GetDes()
        {
            return "Clear Daily";
        }
    }
}