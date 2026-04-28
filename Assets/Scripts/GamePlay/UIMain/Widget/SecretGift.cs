using System.Collections.Generic;
using Foundation;
using Foundation.Statistics;
using Foundation.Storage;
using GameConfig;
using GamePlay.Activity;
using GamePlay.Storage;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class SecretGiftWidget : UIWidget
    {
        [UIBinder("Btn")] private Button open;
        public override void OnCreate()
        {
            base.OnCreate();
            open.onClick.AddListener(() =>
            {
                UIModule.Instance.ShowAsync<SecretView2>();
                
                var dic = new Dictionary<string, object>();
                dic.Add("pos", "SecretGiftEntrance");
                TBAMgr.Instance.SendLogEvent("guide", dic);  
                UIModule.Instance.Close<UIGuide>();
            });
            SubScribeEvent<BuySecret>(OnBuy);
            SubScribeEvent<UpdateActivityIcon>(OnUpdateActivityIcon);
        }

       
        
        private void OnUpdateActivityIcon(UpdateActivityIcon obj)
        {
            ChangeActive();
        }
        private void OnBuy(BuySecret obj)
        {
            gameObject.SetActive(false);
        }

        public override void ChangeActive()
        {
            base.ChangeActive();
            
            if (!DownloadUtility.Instance.activityTags.ContainsKey(PhotoType.SecretGift) ||
                DownloadUtility.Instance.activityTags[PhotoType.SecretGift].Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            // var activityInfo = StorageManager.Instance.GetStorage<ActivityInfo>();
            // if (activityInfo.SecretGiftInfo.IsBuySecret)
            // {
            //     gameObject.SetActive(false);
            //     return;
            // } 
            
            
            int level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            if (level >= GameConfigSys.activityOpenLevel[PhotoType.SecretGift])
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
        }
    }
}