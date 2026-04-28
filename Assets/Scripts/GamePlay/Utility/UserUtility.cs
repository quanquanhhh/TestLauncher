using Foundation.Storage;
using GamePlay.Storage;

namespace GamePlay
{
    public class UserUtility
    {
        public static string UserType => StorageManager.Instance.GetStorage<BaseInfo>().UserType;

        public static bool IsVip => StorageManager.Instance.GetStorage<BaseInfo>().Buff
            .IsVip;

        public static bool IsLifeVip => StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent;
    }
}