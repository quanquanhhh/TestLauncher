using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GamePlay.Storage;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class RemoveADS : UIWidget
    {
        [UIBinder("Btn")] private Button open;

        private void OnVIPStateChange(VIPStateChange obj)
        {
            if (!StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            open.onClick.AddListener(OpenRemoveAds); 
            SubScribeEvent<AdsStateChange>(OnRemove);
            SubScribeEvent<VIPStateChange>(OnVIPStateChange);
        }

        private void OnRemove(AdsStateChange obj)
        {
            gameObject.SetActive(false);
        }

        private void OpenRemoveAds()
        {
            UIModule.Instance.ShowAsync<RemoveADSView>();
        }
    }
}