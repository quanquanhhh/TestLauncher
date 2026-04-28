using Foundation;
using Foundation.Storage;
using GamePlay.Storage;

namespace GamePlay.UIMain.Widget
{
    public class Vip : UIWidget
    {
        public override void OnCreate()
        {
            base.OnCreate(); 
            SubScribeEvent<VIPStateChange>(OnVIPStateChange);
        }

        private void OnVIPStateChange(VIPStateChange obj)
        {
            bool v = StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent;
            gameObject.SetActive(!v);
        }
    }
}