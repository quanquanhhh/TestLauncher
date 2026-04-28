using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using Unity.VisualScripting;

namespace GamePlay.DebugPanel
{
    public class ClearPhotoInfo : DebugCell
    {
        public PhotoInfo info = StorageManager.Instance.GetStorage<PhotoInfo>();
        public override void DebugFun()
        {
            base.DebugFun();
            info.Clear();
            // activityInfo.SignInfo.Clear(); 
        }

        public override string GetDes()
        {
            return "Clear Photo";
        }
    }
    public class ChangeUserTypeFun : DebugCell
    {
        public BaseInfo info = StorageManager.Instance.GetStorage<BaseInfo>();
        public override void DebugFun()
        {
            base.DebugFun();
            if (info.UserType == "A")
            {
                info.UserType = "B";
            }
            else
            {
                info.UserType = "A";
            }
            Event.Instance.SendEvent(new ChangeUserType());
        }

        public override string GetDes()
        {
            return "A/B";
        }
    }
}